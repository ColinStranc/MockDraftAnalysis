using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class WProspect
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public WTeam Team { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Position { get; set; }
        public string Handedness { get; set; }
        public DateTime BirthDay { get; set; }
        public int DraftYear { get; set; }
        public string BirthCity { get; set; }
        public string BirthCountry { get; set; }
        public string Notes { get; set; }
    }
}