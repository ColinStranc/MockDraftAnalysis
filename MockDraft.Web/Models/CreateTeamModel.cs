using AutoMapper;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class CreateTeamModel : CreateModel
    {
        private IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

        public List<WLeague> PossibleLeagues;

        public WTeam TeamModel { get; set; }
        public int LeagueId { get; set; }

        public CreateTeamModel()
        {
            TeamModel = new WTeam();

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