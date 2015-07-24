using AutoMapper;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class CreateProspectModel : CreateModel
    {
        //private I

        public List<WTeam> PossibleTeams;

        public WProspect ProspectModel { get; set; }
        public int TeamId { get; set; }

        public override string ModelType { get { return "Prospect"; } }
        public override string ModelName { get { return ProspectModel.Name; } }

        public CreateProspectModel()
        {
            ProspectModel = new WProspect();

            var dTeams = db.GetAllTeams();
            PossibleTeams = new List<WTeam>();
            foreach (var dTeam in dTeams)
            {
                var wTeam = Mapper.Map<WTeam>(dTeam);
                PossibleTeams.Add(wTeam);
            }
        }
    }
}