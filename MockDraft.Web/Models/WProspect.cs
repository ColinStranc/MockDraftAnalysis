using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class WProspect
    {
        /*
        public int Id;
        public string Name;
        public string Team;
        public string Position;
        public string Handedness;
        public int DraftYear;
        */

        public int Id;
        public string Name;
        public WTeam Team;
        public int Height;
        public int Weight;
        public string Position;
        public string Handedness;
        public DateTime BirthDay;
        public int DraftYear;
        public string BirthCity;
        public string BirthCountry;
        public string Notes;
    }
}