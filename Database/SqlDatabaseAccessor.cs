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

        public void AddTeam(DTeam team)
        {
            var dbInteractor = new DatabaseInteractions(ConnectionString);
            dbInteractor.AddTeam(team);
        }

        public bool LeagueNameExists(string name)
        {
            var dbInteractor = new DatabaseInteractions(ConnectionString);
            var existingLeague = dbInteractor.GetLeague(name);

            if (existingLeague == null) return false;

            return true;
        }

        public bool TeamExists(DTeam team)
        {
            var dbInteractor = new DatabaseInteractions(ConnectionString);
            var existingTeam = dbInteractor.GetTeam(team.Name, team.League.Id);

            if (existingTeam == null) return false;

            return true;
        }

        public List<DLeague> GetAllLeagues()
        {
            var dbInteractor = new DatabaseInteractions(ConnectionString);
            var leagues = dbInteractor.GetAllLeagues();
            return leagues;
        }
    }
}
