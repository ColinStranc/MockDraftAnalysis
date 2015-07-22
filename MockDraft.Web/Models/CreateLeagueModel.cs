using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class CreateLeagueModel : CreateModel
    {
        public WLeague LeagueModel { get; set; }

        public override string ModelType { get { return "League"; } }
        public override string ModelName { get { return LeagueModel.Name; } }

        public CreateLeagueModel()
        {
            LeagueModel = new WLeague();
        }
    }
}