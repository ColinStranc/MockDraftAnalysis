using AutoMapper;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class TeamWithPossibleLeagues : WTeam
    {
        private IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

        public List<WLeague> PossibleLeagues;

        public int LeagueId { get; set; }

        public TeamWithPossibleLeagues()
        {

            var dLeagues = db.GetAllLeagues();
            PossibleLeagues = new List<WLeague>();
            foreach (var dLeague in dLeagues)
            {
                var wLeague = Mapper.Map<WLeague>(dLeague);
                PossibleLeagues.Add(wLeague);
            }

        }
    }
}