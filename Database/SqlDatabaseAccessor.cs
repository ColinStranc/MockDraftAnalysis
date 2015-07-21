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
            DatabaseInteractions dbInteractor = new DatabaseInteractions(ConnectionString);
            Queue<DProspect> prospects = dbInteractor.GetProspectsByYearAsQueue(year);

            if (count == -1)
            {
                return ListUtility.QueueToList<DProspect>(prospects);
            }

            List<DProspect> returnedProspects = ListUtility.GetFirstElements<DProspect>(prospects, count);
            
            return returnedProspects;
        }

        public void AddLeague(DLeague league)
        {
            var dbInteractor = new DatabaseInteractions(ConnectionString);
            dbInteractor.AddLeague(league);
        }
    }
}
