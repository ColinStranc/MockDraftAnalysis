using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseModels
{
    public class DProspect
    {
        public int Id;
        public string Name;
        public DTeam Team;
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
