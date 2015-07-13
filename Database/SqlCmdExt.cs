using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using log4net;
using System.Configuration;

namespace Database
{
    /// <summary>
    /// This class is a wrapper around SqlConnection, SqlCommand and SqlDataReader.
    /// It simplifies the Closing of all the resources with a single using() block.
    /// </summary>
    /// 
    /// The Get[Int32,String,...]() methods I thought long and hard about passing
    /// an integer colIndex (which is a little more efficient), or passing the 
    /// column name (which is more readable).  I went for readable, because when
    /// you are getting 10 columns back it's just too easy to get it wrong.
    /// Or when someone changes the stored proc, it's too easy to create a difficult
    /// to detect problem.
    /// 
    /// This class is not multi-thread safe and instances should not be passed
    /// between threads.
    /// 
    /// === Transaction Support ===
    /// The SqlCmdExt can participate in a transaction if it uses the constructor:
    ///   SqlCmdExt(SqlConnection sqlConnection, SqlTransaction sqlTransaction)
    /// 
    public sealed class SqlCmdExt : IDisposable
    {
        /**********************************************************************/
        #region Data members.

        /// <summary>
        /// The standard logger for SQL operations.
        /// </summary>
        private static readonly ILog standardSqlLog = LogManager.GetLogger("Standard.SQL");

        /// <summary>
        /// The class logger, this is used for logging errors because 
        /// Standard.SQL may well be filtered from the regular logs.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SqlCmdExt));

        private readonly DateTime openTime = DateTime.Now;
        private readonly int commandTimeoutInSeconds;
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private readonly StringBuilder argsText;
        private readonly SqlTransaction sqlTransaction;
        private readonly bool doNotCloseConnection;
        private SqlConnection sqlConnection;
        private SqlCommand sqlCommand;
        private SqlDataReader sqlReader;
        private string logPrefix;
        private string errorMsg;
        private long execDuration;
        private int rowCount;
        private int returnValue;


        #endregion

        /**********************************************************************/
        #region Properties.

        public SqlConnection Connection { get { return sqlConnection; } }
        public DateTime OpenTime { get { return openTime; } }
        public string Type { get { return logPrefix; } }
        public long ExecDuration { get { return execDuration; } }
        public long OpenDuration { get { return stopwatch.ElapsedMilliseconds; } }
        public int RowCount { get { return rowCount; } }
        public string Command { get { return sqlCommand != null ? sqlCommand.CommandText : ""; } }
        public string ArgsSummary { get { return argsText == null ? "" : argsText.ToString(); } }

        /// <summary>
        /// The exception message if there was an exception while executing.
        /// </summary>
        public string Error { get { return errorMsg; } }

        /// <summary>
        /// Do not log an execution error.
        /// </summary>
        public bool DoNotLogError { get; set; }

        /// <summary>
        /// The InitialCatalog associated with the connection.
        /// </summary>
        public string InitialCatalog { get { return GetInitialCatalog(); } }

        /// <summary>
        /// It is quite common that the QEP that SQL server generate when it
        /// recieves parameters (for SELECTS) is ok, but not great.  
        /// This is probably because it cannot estimate the index coverage accurately, so
        /// it must choose a safer approach.
        /// 
        /// EnableParamSubstitution will allow SqlCmdExt to quitely replace 
        /// parameters in SQL when you call SetInArg().
        /// This will force SQL server to generate a new QEP and allow it to better
        /// use the index statistics.
        /// 
        /// Do not use it for stored procs because it is a simple replace in the query string.
        /// </summary>
        public bool EnableParamSubstitution { get; set; }

        #endregion

        /**********************************************************************/
        #region Construtor / Dispose.

        /// <summary>
        /// Connect with a connection string.
        /// </summary>
        public SqlCmdExt(string connectionNameg)
        {
            logPrefix = "";

            // If we shall be debugging record the arguments.
            if (standardSqlLog.IsDebugEnabled)
                argsText = new StringBuilder();

            // Open the connection...
            var connectionString = ConfigurationManager.ConnectionStrings[connectionNameg].ConnectionString;

            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }

        /// <summary>
        /// Create a SqlCmdExt that will be part of a transaction.
        /// </summary>
        public SqlCmdExt(SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {
            logPrefix = "";

            // If we shall be debugging record the arguments.
            if (standardSqlLog.IsDebugEnabled)
                argsText = new StringBuilder();

            // Register the connection and transaction.
            this.sqlConnection = sqlConnection;
            this.sqlTransaction = sqlTransaction;

            // We do not own this connection, so we do not close it.
            doNotCloseConnection = true;
        }

        /// <summary>
        /// Explicit cleanup.
        /// </summary>
        public void Dispose()
        {
            // Dispose pattern.
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Delayed (last chance) cleanup in case someone forgot to call Dispose()
        /// or use a using block.
        /// </summary>
        ~SqlCmdExt()
        {
            if (sqlConnection != null)
            {
                log.Warn("SqlCmdExt was not disposed.");
                ReleaseResources();
            }
        }

        /// <summary>
        /// Releases all resources held by the SqlCmdExt.
        /// </summary>
        /// See: http://msdn.microsoft.com/en-us/library/b1yfkh5e.aspx
        public void ReleaseResources()
        {
            // If we have been cleaned up already sqlConnection will be null.
            if (sqlConnection != null)
            {
                stopwatch.Stop();

                if (standardSqlLog.IsDebugEnabled)
                {
                    standardSqlLog.Debug(this);
                }

                if (sqlReader != null)
                {
                    // Ignore errors because we are done with it...
                    try
                    {
                        sqlReader.Close();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                    sqlReader = null;
                }

                sqlCommand = null;

                // We must explicitly close the connection.
                if (sqlConnection != null && !doNotCloseConnection)
                {
                    // Ignore errors because we are done with it...
                    try
                    {
                        sqlConnection.Close();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                    sqlConnection = null;
                }
            }
        }

        #endregion

        /**********************************************************************/
        #region Object overrides.

        public override string ToString()
        {
            long openDuration = stopwatch.ElapsedMilliseconds;

            return string.Format(
                    "{0}: Exec:{1}ms Open:{2}ms Rows:{3} : {4}",
                    logPrefix,
                    execDuration,
                    openDuration,
                    rowCount,
                    sqlCommand == null ? "No Command" : sqlCommand.CommandText
                    );
        }

        #endregion

        /**********************************************************************/
        #region Creating and executing a command.

        /// <summary>
        /// Set the isolation level for the connection.
        /// This must be called prior to invoking the command.
        /// </summary>
        public void SetIsolationLevel(IsolationLevel level)
        {
            if (sqlConnection == null)
                throw new Exception("Connection has not been opened.");
            if (sqlCommand != null)
                throw new Exception("SetIsolationLevel must be called before creating the command.");
            if (sqlTransaction != null)
                throw new Exception("SetIsolationLevel cannot be used when the SqlCmdExt is part of a transaction.");

            // This relies on a little connection weirdness that the change
            // to the transaction level remains after the commit happens.
            // So it should be in place for our command.
            sqlConnection.BeginTransaction(level).Commit();
        }

        /// <summary>
        /// Switch the isolation level to READ UNCOMMITTED.
        /// </summary>
        public void UseReadUncommitted()
        {
            SetIsolationLevel(IsolationLevel.ReadUncommitted);
        }

        /// <summary>
        /// Create the SqlCommand using the specified command text.
        /// </summary>
        public void CreateCmd(string cmd)
        {
            if (sqlCommand != null)
                throw new Exception("Cannot reuse a SqlCmdExt");
            if (cmd == null)
                throw new ArgumentNullException();

            // Create the command.
            sqlCommand = new SqlCommand(cmd);
            if (commandTimeoutInSeconds > 0)
                sqlCommand.CommandTimeout = commandTimeoutInSeconds;
            sqlCommand.Connection = sqlConnection;
            if (sqlTransaction != null)
                sqlCommand.Transaction = sqlTransaction;
        }

        /// <summary>
        /// Create the SqlCommand using the specified format string and arguments.
        /// </summary>
        public void CreateCmdFormat(string cmdFormat, params object[] args)
        {
            CreateCmd(string.Format(cmdFormat, args));
        }

        /// <summary>
        /// Execute the stored procedure.
        /// </summary>
        public void ExecuteStoredProc()
        {
            if (sqlCommand == null)
                throw new Exception("No call to CreateCmd");
            if (sqlReader != null)
                throw new Exception("Cannot reuse a SqlCmdExt");

            try
            {
                logPrefix = "SQL-SP";

                // Always watch for a return value.
                SqlParameter returnValueParam = sqlCommand.Parameters.Add("@ReturnValue", SqlDbType.Int);

                returnValueParam.Direction = ParameterDirection.ReturnValue;

                // Execute the stored procedure.
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlReader = sqlCommand.ExecuteReader();

                // Not all stored procs return a value so we are careful.
                int? rv = returnValueParam.Value as int?;

                returnValue = rv == null ? 0 : rv.Value;
            }
            catch (Exception e)
            {
                logPrefix = "SQL-SP-FAIL";
                errorMsg = e.Message;
                if (!DoNotLogError)
                    log.Error(e.Message);
                throw;
            }
            finally
            {
                execDuration = stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Execute the SQL text.
        /// </summary>
        public void ExecuteSelect()
        {
            if (sqlCommand == null)
                throw new Exception("No call to CreateCmd");
            if (sqlReader != null)
                throw new Exception("Cannot reuse a SqlCmdExt");

            try
            {
                logPrefix = "SQL";

                // Execute the stored procedure.
                sqlCommand.CommandType = CommandType.Text;
                sqlReader = sqlCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                logPrefix = "SQL-FAIL";
                errorMsg = e.Message;
                if (!DoNotLogError)
                {
                    // It should be reasonably safe to log a SELECT command.
                    // writing INSERT exceptions could leave customer data 
                    // floating around in log files.
                    log.Error("SELECT FAILED\n" + sqlCommand.CommandText, e);
                }
                throw;
            }
            finally
            {
                execDuration = stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Executes the SQL text and loads the results into the specified data table.
        /// </summary>
        public void ExecuteSelect(DataTable data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            ExecuteSelect();

            data.BeginLoadData();
            data.Load(sqlReader);
            data.EndLoadData();
        }

        /// <summary>
        /// Execute the SQL text.
        /// </summary>
        public int ExecuteInsertUpdateDelete(bool ignoreDuplicateKeyFail = false)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            try
            {
                logPrefix = "SQL";

                // Execute the stored procedure.
                sqlCommand.CommandType = CommandType.Text;
                return rowCount = sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                if (ignoreDuplicateKeyFail && e.Message.Contains("Cannot insert duplicate key"))
                    return 0;

                logPrefix = "SQL-FAIL";
                errorMsg = e.Message;
                if (!DoNotLogError)
                    log.Error(e.Message);
                throw;
            }
            finally
            {
                execDuration = stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Execute Non-Query.
        /// </summary>
        public int ExecuteNonQuery()
        {
            return ExecuteInsertUpdateDelete();
        }

        /// <summary>
        /// Execute the SQL text.
        /// </summary>
        public object ExecuteScalar()
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            try
            {
                logPrefix = "SQL";

                // Execute the stored procedure.
                sqlCommand.CommandType = CommandType.Text;
                return sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                logPrefix = "SQL-FAIL";
                errorMsg = e.Message;
                if (!DoNotLogError)
                    log.Error(e.Message);
                throw;
            }
            finally
            {
                execDuration = stopwatch.ElapsedMilliseconds;
            }
        }

        #endregion

        /**********************************************************************/
        #region Setting IN Arguments.

        /// <summary>
        /// Set a bool IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            bool value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, value.ToString());
        }

        /// <summary>
        /// Set a bool IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            bool? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, value.ToString());
            }
        }

        /// <summary>
        /// Set a byte IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            byte value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, value.ToString());
        }

        /// <summary>
        /// Set a byte IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            byte? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, value.ToString());
            }
        }

        /// <summary>
        /// Set a short IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            short value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, value.ToString());
        }

        /// <summary>
        /// Set a short IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            short? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, value.ToString());
            }
        }

        /// <summary>
        /// Set an int IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            int value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, value.ToString());
        }

        /// <summary>
        /// Set an decimal IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            decimal value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, value.ToString());
        }

        /// <summary>
        /// Set an decimal? IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            decimal? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, value.ToString());
            }
        }

        /// <summary>
        /// Set an int IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            int? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, value.ToString());
            }
        }

        /// <summary>
        /// Set a char IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            char value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, "'" + value.ToString() + "'");
        }

        /// <summary>
        /// Set a char IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            char? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, "'" + value.ToString() + "'");
            }
        }

        /// <summary>
        /// Set a string IN argument, gracefully deals with null.
        /// </summary>
        public void SetInArg(
            string argName,
            string value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (EnableParamSubstitution && value != null)
            {
                // Cannot get here with NULL because it is not a simple replace.
                SetInArgParamSub(argName, value);
            }
            else
            {
                SetInArgNoParamSub(argName, value);
            }

        }

        private void SetInArgParamSub(string argName, string value)
        {
            sqlCommand.CommandText = sqlCommand.CommandText.Replace(
                argName,
                "'" + EscapeSqlString(value) + "'"
                );
        }

        private void SetInArgNoParamSub(string argName, string value)
        {
            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value);

                if (argsText != null)
                    AddArgsText(argName, "'" + value + "'");
            }
        }

        /// <summary>
        /// Set a DateTime IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            DateTime value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (EnableParamSubstitution)
            {
                sqlCommand.CommandText = sqlCommand.CommandText.Replace(
                    argName,
                    value.ToString(@"\'yyyy-MM-dd HH:mm:ss.fff\'")
                    );
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value);

                if (argsText != null)
                    AddArgsText(argName, "'" + value.ToString(@"\'yyyy-MM-dd HH:mm:ss.fff\'") + "'");
            }
        }

        /// <summary>
        /// Set a nullable DateTime IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            DateTime? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                SetInArg(argName, value.Value);
            }
        }

        /// <summary>
        /// Set a GUID IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            Guid value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            sqlCommand.Parameters.AddWithValue(argName, value);

            if (argsText != null)
                AddArgsText(argName, "'" + value.ToString() + "'");
        }

        /// <summary>
        /// Set a GUID IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            Guid? value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value.Value);

                if (argsText != null)
                    AddArgsText(argName, "'" + value.ToString() + "'");
            }
        }

        /// <summary>
        /// Set an byte[] IN argument.
        /// </summary>
        public void SetInArg(
            string argName,
            byte[] value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.Add(argName, SqlDbType.VarBinary).Value = DBNull.Value;

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.Add(argName, SqlDbType.VarBinary, value.Length).Value = value;

                if (argsText != null)
                    AddArgsText(argName, "byte[" + value.Length + "]");
            }
        }

        /// <summary>
        /// Set a generic in argument.
        /// </summary>
        public void SetInArgObject(
            string argName,
            object value
            )
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");
            if (argName == null)
                throw new ArgumentNullException("argName");

            if (value == null)
            {
                sqlCommand.Parameters.AddWithValue(argName, DBNull.Value);

                if (argsText != null)
                    AddArgsText(argName, "NULL");
            }
            else
            {
                sqlCommand.Parameters.AddWithValue(argName, value);

                if (argsText != null)
                    AddArgsText(argName, "'" + value + "'");
            }
        }

        #endregion

        /**********************************************************************/
        #region Iterating over the results.

        /// <summary>
        /// The return value from the stored procedure.
        /// </summary>
        public int GetReturnValue()
        {
            return returnValue;
        }

        /// <summary>
        /// Advances the sqlReader to the first/next row.
        /// </summary>
        public bool Read()
        {
            if (sqlReader.Read())
            {
                rowCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves to the next result set in the result.
        /// This is only needed to move past the first result set.
        /// </summary>
        public bool NextResultSet()
        {
            return sqlReader.NextResult();
        }

        /// <summary>
        /// If you really want direct access to the reader.
        /// </summary>
        /// <returns></returns>
        public SqlDataReader GetDataReader()
        {
            return sqlReader;
        }

        #endregion

        /**********************************************************************/
        #region Reading from the current row.

        /// <summary>
        /// Find the ordinal number for a column name.
        /// </summary>
        public int GetOrdinal(string argName)
        {
            return sqlReader.GetOrdinal(argName);
        }

        public bool IsNull(string argName)
        {
            return IsNull(GetOrdinal(argName));
        }

        public bool IsNull(int colIndex)
        {
            return sqlReader.IsDBNull(colIndex);
        }


        /// <summary>
        /// Get a bool from the current row in the result set.
        /// </summary>
        public bool GetBool(string argName)
        {
            return GetBool(GetOrdinal(argName));
        }

        public bool GetBool(string argName, bool defaultValue)
        {
            return GetBool(GetOrdinal(argName), defaultValue);
        }

        public bool? GetNullableBool(string argName)
        {
            return GetNullableBool(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a bool from the current row in the result set.
        /// </summary>
        public bool GetBool(int colIndex)
        {
            return sqlReader.GetBoolean(colIndex);
        }

        public bool? GetNullableBool(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetBoolean(colIndex);
        }

        public bool GetBool(int colIndex, bool defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetBoolean(colIndex);
        }

        /// <summary>
        /// Get a byte from the current row in the result set.
        /// </summary>
        public byte GetByte(string argName)
        {
            return GetByte(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a byte from the current row in the result set.
        /// </summary>
        public byte GetByte(string argName, byte defaultValue)
        {
            return GetByte(GetOrdinal(argName), defaultValue);
        }

        /// <summary>
        /// Get a byte from the current row in the result set.
        /// </summary>
        public byte GetByte(int colIndex)
        {
            return sqlReader.GetByte(colIndex);
        }

        /// <summary>
        /// Get a byte from the current row in the result set.
        /// </summary>
        public byte GetByte(int colIndex, byte defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetByte(colIndex);
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short GetShort(string argName)
        {
            return GetShort(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short GetShort(string argName, short defaultValue)
        {
            return GetShort(GetOrdinal(argName), defaultValue);
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short GetShort(int colIndex)
        {
            return sqlReader.GetInt16(colIndex);
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short GetShort(int colIndex, short defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetInt16(colIndex);
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short? GetNullableShort(string argName)
        {
            return GetNullableShort(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a short from the current row in the result set.
        /// </summary>
        public short? GetNullableShort(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetInt16(colIndex);
        }

        /// <summary>
        /// Get a int from the current row in the result set.
        /// </summary>
        public int GetInt(string argName)
        {
            return GetInt(GetOrdinal(argName));
        }

        public int GetInt(string argName, int defaultValue)
        {
            return GetInt(GetOrdinal(argName), defaultValue);
        }

        /// <summary>
        /// Get a int from the current row in the result set.
        /// </summary>
        public int GetInt(int colIndex)
        {
            return sqlReader.GetInt32(colIndex);
        }

        public int GetInt(int colIndex, int defaultValue)
        {
            return sqlReader.IsDBNull(colIndex)
                ? defaultValue
                : sqlReader.GetInt32(colIndex);
        }

        /// <summary>
        /// Get a nullable DateTime from the current row in the result set.
        /// </summary>
        public int? GetNullableInt(string argName)
        {
            return GetNullableInt(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a nullable DateTime from the current row in the result set.
        /// </summary>
        public int? GetNullableInt(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetInt32(colIndex);
        }

        /// <summary>
        /// Get a long from the current row in the result set.
        /// </summary>
        public long GetLong(string argName)
        {
            return GetLong(GetOrdinal(argName));
        }

        public long GetLong(string argName, long defaultValue)
        {
            return GetLong(GetOrdinal(argName), defaultValue);
        }


        /// <summary>
        /// Get a int from the current row in the result set.
        /// </summary>
        public long GetLong(int colIndex)
        {
            return sqlReader.GetInt64(colIndex);
        }

        public long GetLong(int colIndex, long defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetInt64(colIndex);
        }

        /// <summary>
        /// Get a nullable long from the current row in the result set.
        /// </summary>
        public long? GetNullableLong(string argName)
        {
            return GetNullableLong(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a nullable long from the current row in the result set.
        /// </summary>
        public long? GetNullableLong(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetInt64(colIndex);
        }

        /// <summary>
        /// Get a decimal from the current row in the result set.
        /// </summary>
        public decimal GetDecimal(string argName)
        {
            return GetDecimal(GetOrdinal(argName));
        }

        public decimal? GetNullableDecimal(string argName)
        {
            return GetNullableDecimal(GetOrdinal(argName));
        }

        public decimal GetDecimal(string argName, decimal defaultValue)
        {
            return GetDecimal(GetOrdinal(argName), defaultValue);
        }

        /// <summary>
        /// Get a decimal from the current row in the result set.
        /// </summary>
        public decimal GetDecimal(int colIndex)
        {
            return sqlReader.GetDecimal(colIndex);
        }

        public decimal? GetNullableDecimal(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetDecimal(colIndex);
        }

        public decimal GetDecimal(int colIndex, decimal defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetDecimal(colIndex);
        }

        /// <summary>
        /// Get a Float.
        /// </summary>
        public float GetFloat(string argName)
        {
            return GetFloat(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a Float.
        /// </summary>
        public float GetFloat(int colIndex)
        {
            return sqlReader.GetFloat(colIndex);
        }

        public float GetFloat(string argName, float defaultValue)
        {
            return GetFloat(GetOrdinal(argName), defaultValue);
        }

        public float GetFloat(int colIndex, float defaultValue)
        {
            return sqlReader.IsDBNull(colIndex)
                ? defaultValue
                : sqlReader.GetFloat(colIndex);
        }

        /// <summary>
        /// Get a Double.
        /// </summary>
        public Double GetDouble(string argName)
        {
            return GetDouble(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a Double.
        /// </summary>
        public Double GetDouble(int colIndex)
        {
            return sqlReader.GetDouble(colIndex);
        }

        public Double GetDouble(string argName, Double defaultValue)
        {
            return GetDouble(GetOrdinal(argName), defaultValue);
        }

        public Double GetDouble(int colIndex, Double defaultValue)
        {
            return sqlReader.IsDBNull(colIndex)
                ? defaultValue
                : sqlReader.GetDouble(colIndex);
        }

        /// <summary>
        /// Get a char from the current row in the result set.
        /// </summary>
        public char GetChar(string argName)
        {
            return GetChar(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a char from the current row in the result set.
        /// </summary>
        public char GetChar(int colIndex)
        {
            char[] ca = new char[1];

            return (sqlReader.GetChars(colIndex, 0, ca, 0, 1) == 1) ? ca[0] : '\0';
        }

        /// <summary>
        /// Get a nullable char from the current row in the result set.
        /// </summary>
        public char? GetNullableChar(string argName)
        {
            return GetNullableChar(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a nullable char from the current row in the result set.
        /// </summary>
        public char? GetNullableChar(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;

            char[] ca = new char[1];
            if (sqlReader.GetChars(colIndex, 0, ca, 0, 1) == 1)
                return ca[0];

            return null;
        }

        /// <summary>
        /// Get a string from the current row in the result set, cannot be null.
        /// </summary>
        public string GetString(string argName)
        {
            return GetString(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a string from the current row in the result set, cannot be null.
        /// </summary>
        public string GetString(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetString(colIndex);
        }

        /// <summary>
        /// Get a string from the current row in the result set, 
        /// if the value is null use the replacement.
        /// </summary>
        public string GetString(string argName, string nullReplacementValue)
        {
            return GetString(GetOrdinal(argName), nullReplacementValue);
        }

        /// <summary>
        /// Get a string from the current row in the result set, 
        /// if the value is null use the replacement.
        /// </summary>
        public string GetString(int colIndex, string nullReplacementValue)
        {
            return sqlReader.IsDBNull(colIndex) ? nullReplacementValue : sqlReader.GetString(colIndex);
        }

        /// <summary>
        /// Get a DateTime from the current row in the result set.
        /// </summary>
        public DateTime GetDateTime(string argName)
        {
            return GetDateTime(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a DateTime from the current row in the result set.
        /// </summary>
        public DateTime GetDateTime(string argName, DateTime defaultValue)
        {
            return GetDateTime(GetOrdinal(argName), defaultValue);
        }

        /// <summary>
        /// Get a DateTime from the current row in the result set.
        /// </summary>
        public DateTime GetDateTime(int colIndex)
        {
            return sqlReader.GetDateTime(colIndex);
        }

        /// <summary>
        /// Get a DateTime from the current row in the result set.
        /// </summary>
        public DateTime GetDateTime(int colIndex, DateTime defaultValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return defaultValue;
            return sqlReader.GetDateTime(colIndex);
        }

        /// <summary>
        /// Get a nullable DateTime from the current row in the result set.
        /// </summary>
        public DateTime? GetNullableDateTime(string argName)
        {
            return GetNullableDateTime(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a nullable DateTime from the current row in the result set.
        /// </summary>
        public DateTime? GetNullableDateTime(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetDateTime(colIndex);
        }

        /// <summary>
        /// Get a Guid from the current row in the result set.
        /// </summary>
        public Guid GetGuid(string argName)
        {
            return GetGuid(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a Guid from the current row in the result set.
        /// </summary>
        public Guid GetGuid(int colIndex)
        {
            return sqlReader.GetGuid(colIndex);
        }

        /// <summary>
        /// Get a Guid from the current row in the result set.
        /// </summary>
        public Guid? GetNullableGuid(string argName)
        {
            return GetNullableGuid(GetOrdinal(argName));
        }

        /// <summary>
        /// Get a Guid from the current row in the result set.
        /// </summary>
        public Guid? GetNullableGuid(int colIndex)
        {
            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetGuid(colIndex);
        }

        /// <summary>
        /// Get a byte[] from the current row in the result set, 
        /// if the value is null use the replacement.
        /// </summary>
        public byte[] GetBinary(string argName, byte[] nullReplacementValue)
        {
            return GetBinary(GetOrdinal(argName), nullReplacementValue);
        }

        /// <summary>
        /// Get a string from the current row in the result set, 
        /// if the value is null use the replacement.
        /// </summary>
        public byte[] GetBinary(int colIndex, byte[] nullReplacementValue)
        {
            if (sqlReader.IsDBNull(colIndex))
                return nullReplacementValue;

            using (MemoryStream ms = new MemoryStream(2 * 1024))
            {
                byte[] buffer = new byte[2 * 1024];
                long len;
                long dataIndex = 0;

                while ((len = sqlReader.GetBytes(colIndex, dataIndex, buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, (int)len);
                    dataIndex += len;
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get an Object.
        /// </summary>
        public Object GetObject(String argName)
        {
            return GetObject(GetOrdinal(argName));
        }

        public Object GetObject(int colIndex)
        {

            if (sqlReader.IsDBNull(colIndex))
                return null;
            return sqlReader.GetValue(colIndex);
        }

        #endregion

        /**********************************************************************/
        #region Register OUT arguments.

        public void RegisterOutArgBool(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.Bit);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgByte(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.TinyInt);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgShort(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.SmallInt);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgInt(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.Int);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgChar(string argName, int size)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.Char, size);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgVarchar(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.VarChar);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgText(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.Text);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgGuid(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.UniqueIdentifier);

            param.Direction = ParameterDirection.Output;
        }

        public void RegisterOutArgDateTime(string argName)
        {
            if (sqlCommand == null)
                throw new Exception("No command specified.");

            SqlParameter param = sqlCommand.Parameters.Add(argName, SqlDbType.DateTime);

            param.Direction = ParameterDirection.Output;
        }

        #endregion

        /**********************************************************************/
        #region Read OUT argument.

        public bool GetOutArgBool(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return false;
            return (bool)sqlParam.Value;
        }

        public byte GetOutArgByte(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return 0;
            return (byte)sqlParam.Value;
        }

        public short GetOutArgShort(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return 0;
            return (short)sqlParam.Value;
        }

        public int GetOutArgInt(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return 0;
            return (int)sqlParam.Value;
        }

        public string GetOutArgString(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return null;
            return (string)sqlParam.Value;
        }

        public DateTime? GetOutArgDateTime(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return null;
            return (DateTime)sqlParam.Value;
        }

        public Guid? GetOutArgGuid(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
                return null;
            return (Guid)sqlParam.Value;
        }

        /*
        public char GetOutArgChar(string argName)
        {
            SqlParameter sqlParam = sqlCommand.Parameters[argName];

            if (sqlParam.Value == DBNull.Value)
            {
                return '\0';
            }
            else
            {
                string s = sqlParam.Value as string;

                return s.Length > 0 ? s[0] : '\0';
            }
        }
        */

        #endregion

        /**********************************************************************/
        #region Column Information Methods.

        public int ColCount { get { return sqlReader.FieldCount; } }

        public string GetColName(int i)
        {
            return sqlReader.GetName(i);
        }

        public Type GetColType(int i)
        {
            return sqlReader.GetFieldType(i);
        }

        public string GetSqlTypeName(int i)
        {
            return sqlReader.GetDataTypeName(i);
        }

        public DataTable GetSchemaTable()
        {
            return sqlReader.GetSchemaTable();
        }

        #endregion

        /**********************************************************************/
        #region Support Methods.

        private static Regex MASKED_ARG_NAMES = new Regex(
            ".*(account|password).*", RegexOptions.IgnoreCase
            );

        private void AddArgsText(string argName, string argValue)
        {
            // Mask out values for arguments that could be confidential.
            if (MASKED_ARG_NAMES.IsMatch(argName))
                argValue = "####";

            // Truncate anything too long.
            if (argValue.Length > 38)
                argValue = argValue.Substring(0, 10) + "...'";

            if (argsText.Length > 0)
                argsText.Append(", ");

            argsText.Append(argName);
            argsText.Append('=');
            argsText.Append(argValue);
        }

        /// <summary>
        /// Creates a nicely formatted summary of the current row of data.
        /// </summary>
        /// <returns></returns>
        public string DescribeCurrentRow()
        {
            StringBuilder sb = new StringBuilder();
            List<string[]> colData = new List<string[]>();
            int maxIndex = 0;
            int maxColNameLen = 0;
            int maxSqlTypeNameLen = 0;
            int maxDotNetTypeNameLen = 0;

            // This is easier to read when it is in alphabetical order, not ordinal order...
            List<string> colNames = new List<string>();

            for (int i = 0; i < sqlReader.FieldCount; i++)
                colNames.Add(sqlReader.GetName(i));
            colNames.Sort();

            foreach (string colName in colNames)
            {
                int i = sqlReader.GetOrdinal(colName);
                string index = i.ToString();
                string sqlTypeName = sqlReader.GetDataTypeName(i);
                string dotNetTypeName = "(" + sqlReader.GetFieldType(i).Name + ")";
                string value =
                    sqlReader.IsDBNull(i) ? "[NULL]"
                    : sqlReader.GetFieldType(i) == typeof(String) ? "'" + sqlReader.GetValue(i) + "'"
                    : sqlReader.GetValue(i).ToString();
                string[] data = new[] { index, colName, sqlTypeName, dotNetTypeName, value };

                if (index.Length > maxIndex)
                    maxIndex = index.Length;
                if (colName.Length > maxColNameLen)
                    maxColNameLen = colName.Length;
                if (sqlTypeName.Length > maxSqlTypeNameLen)
                    maxSqlTypeNameLen = sqlTypeName.Length;
                if (dotNetTypeName.Length > maxDotNetTypeNameLen)
                    maxDotNetTypeNameLen = dotNetTypeName.Length;

                colData.Add(data);
            }

            for (int i = 0; i < colData.Count; i++)
            {
                string[] data = colData[i];

                sb.AppendFormat(
                    "{5} [{0}] {1} [{2} {3}] = {4}",
                    data[0].PadLeft(maxIndex), // Index #
                    data[1].PadRight(maxColNameLen), // Column name
                    data[2].PadRight(maxSqlTypeNameLen), // SQL type
                    data[3].PadRight(maxDotNetTypeNameLen), // DotNet type
                    data[4], // Value.
                    i > 0 ? "\n" : ""
                    );
            }

            return sb.ToString();
        }

        public void StoreRow(Dictionary<string, object> data)
        {
            for (int i = 0; i < sqlReader.FieldCount; i++)
            {
                data[sqlReader.GetName(i)] = sqlReader.IsDBNull(i)
                    ? null
                    : sqlReader.GetValue(i);
            }
        }

        /// <summary>
        /// Generate a nice name for the database (mostly for logging reasons).
        /// </summary>
        private string GetInitialCatalog()
        {
            string initialCatalog = "Unknown";

            try
            {
                if (sqlConnection != null)
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(
                        sqlConnection.ConnectionString
                        );

                    initialCatalog = builder.InitialCatalog;
                }
            }
            catch (Exception e)
            {
                log.Error("Unable to determine InitialCatalog: " + e);
            }

            return initialCatalog;
        }

        /// <summary>
        /// Escapes any characters within an SQL text value.
        /// inTextValue = Foo'Bar would be converted to Foo''Bar
        /// </summary>
        public static string EscapeSqlString(string inTextValue)
        {
            if (inTextValue == null)
                return null;

            char[] ca = inTextValue.ToCharArray();

            // Most of the strings do not need escaping, so we shall avoid it
            // if not needed.
            bool needsEscaping = false;

            foreach (char c in ca)
            {
                if (c == '\'')
                {
                    needsEscaping = true;
                    break;
                }
            }

            if (!needsEscaping)
                return inTextValue;

            // Ok, do the escaping.
            StringBuilder sb = new StringBuilder();

            foreach (char c in ca)
            {
                if (c == '\'')
                {
                    sb.Append("''");
                }
                else
                {
                    sb.Append(c);
                }
            }

            // If they did not use a wildcard than do it automatically.
            return sb.ToString();
        }


        #endregion
    }
}

