using System.Collections.Generic;
using DatabaseModels;

namespace Database
{
    public interface IDatabaseAccessor
    {
        void AddLeague(DLeague league);
        void AddTeam(DTeam team);
        void AddProspect(DProspect prospect);
        List<DProspect> GetTopProspects(int year, int count);
        bool LeagueNameExists(string name);
        bool TeamExists(DTeam team);
        bool ProspectExists(DProspect prospect);
        List<DLeague> GetAllLeagues();
        List<DTeam> GetAllTeams();
    }

    public class DatabaseAccessorSpoof : IDatabaseAccessor
    {
        public List<DLeague> GetAllLeagues()
        {
            return null;
        }

        public List<DTeam> GetAllTeams()
        {
            return null;
        }

        public bool ProspectExists(DProspect prospect)
        {
            return false;
        }

        public bool TeamExists(DTeam team)
        {
            return false;
        }

        public bool LeagueNameExists(string name)
        {
            return false;
        }
        public void AddTeam(DTeam team) { }
        public void AddLeague(DLeague league) { }
        public void AddProspect(DProspect prospect) { }
        public List<DProspect> GetTopProspects(int year, int count)
        {
            var allProspects = new Queue<DProspect>();
            
            var returnedProspects = new List<DProspect>();
            if (count == -1) count = allProspects.Count;
            for (int i = 0; i < count; i++)
            {
                if (allProspects.Count == 0) break;
                var prospect = allProspects.Dequeue();
                if (prospect.DraftYear == year)
                {
                    returnedProspects.Add(prospect);
                }
                else i--;
            }

            return returnedProspects;
        }
    }
}
