using System.Collections.Generic;
using AutoMapper;

namespace MockDraft.Web.Models
{
    public class CreateTeamModel : CreateModel
    {
        //private IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

        public List<WLeague> PossibleLeagues;

        public WTeam TeamModel { get; set; }
        public int LeagueId { get; set; }

        public override string ModelType { get { return "Team"; } }
        public override string ModelName { get { return TeamModel.Name; } }

        public CreateTeamModel()
        {
            TeamModel = new WTeam();

            var dLeagues = Db.GetAllLeagues();
            PossibleLeagues = new List<WLeague>();
            foreach (var dLeague in dLeagues)
            {
                var wLeague = Mapper.Map<WLeague>(dLeague);
                PossibleLeagues.Add(wLeague);
            }
        }
    }
}