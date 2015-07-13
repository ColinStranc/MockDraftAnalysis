using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseModels;
using Utility;

namespace Database
{
    public class SqlDatabaseAccessor : IDatabaseAccessor
    {
        public string ConnectionString { get; private set; }

        public SqlDatabaseAccessor(string connectionString)
            : base()
        {
            ConnectionString = connectionString;
        }

        public List<DProspect> GetTopProspects(int year, int count)
        {
            List<DProspect> returnedProspects = new List<DProspect>();
            Queue<DProspect> prospects = new Queue<DProspect>();

            using (var cmd = new SqlCmdExt(ConnectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Prospect
                    WHERE
                        DraftYear = @DraftYear
                ");
                cmd.SetInArg("@DraftYear", year);

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    var dProspect = new DProspect()
                    {
                        Id = cmd.GetInt("Id"),
                        Name = cmd.GetString("Name"),
                        Team = cmd.GetString("Team"),
                        Position = cmd.GetString("Position"),
                        Handedness = cmd.GetString("Handedness"),
                        DraftYear = cmd.GetInt("DraftYear")
                    };
                    prospects.Enqueue(dProspect);
                }
            }

            returnedProspects = ListUtility.GetFirstElements<DProspect>(prospects, count);

            return returnedProspects;
        }
    }
}
