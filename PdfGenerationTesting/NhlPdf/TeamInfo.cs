using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerationTesting.NhlPdf
{
    public class TeamInfo
    {
        public string TeamPictureUrl { get; set; }

        public List<PlayerInfo> Forwards { get; set; }
        public List<PlayerInfo> Defencemen { get; set; }
        public List<PlayerInfo> Goalies { get; set; }

        public TeamInfo()
        {
            Forwards = new List<PlayerInfo>();
            Defencemen = new List<PlayerInfo>();
            Goalies = new List<PlayerInfo>();
        }
    }
}
