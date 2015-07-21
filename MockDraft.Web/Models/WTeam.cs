using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class WTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public WLeague League { get; set; }
    }
}