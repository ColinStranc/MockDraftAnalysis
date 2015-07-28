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
        private DatabaseInteractions db;

        public SqlDatabaseAccessor(string connectionString)
            : base()
        {
            ConnectionString = connectionString;
            db = new DatabaseInteractions(ConnectionString);
        }

        public List<DProspect> GetTopProspects(int year, int count)
        {
            Queue<DProspect> prospects = db.GetProspectsByYearAsQueue(year);

            if (count == -1)
            {
                return ListUtility.QueueToList<DProspect>(prospects);
            }

            List<DProspect> returnedProspects = ListUtility.GetFirstElements<DProspect>(prospects, count);
            
            return returnedProspects;
        }

        public void AddLeague(DLeague league)
        {
            db.AddLeague(league);
        }

        public void AddTeam(DTeam team)
        {
            db.AddTeam(team);
        }

        public void AddProspect(DProspect prospect)
        {
            if (prospect.DraftYear == 0)
            {
                prospect.DraftYear = Utility.Conversions.GetDraftYearFromBirthYear(prospect.BirthDay);
            }

            db.AddProspect(prospect);
        }

        public bool LeagueNameExists(string name)
        {
            try
            {
                var existingLeague = db.GetLeague(name);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool TeamExists(DTeam team)
        {
            try
            {
                var existingTeam = db.GetTeam(team.Name, team.League.Id);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool ProspectExists(DProspect prospect)
        {
            try
            {
                var existingProspect = db.GetProspect(prospect.Name, prospect.BirthDay);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public List<DLeague> GetAllLeagues()
        {
            var leagues = db.GetAllLeagues();
            return leagues;
        }

        public List<DTeam> GetAllTeams()
        {
            var teams = db.GetAllTeams();
            return teams;
        }
    }
}
