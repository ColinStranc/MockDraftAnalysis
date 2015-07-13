using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseModels;

namespace Database
{
    public static class DatabaseInteractions
    {
        public static Queue<DProspect> GetProspectsByYearAsQueue(int year, string connectionString)
        {
            Queue<DProspect> prospects = new Queue<DProspect>();

            using (var cmd = new SqlCmdExt(connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Prospect
                    WHERE
                        DraftYear = @DraftYear
                ");
                cmd.SetInArg("@DraftYear", year);

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    var dProspect = new DProspect()
                    {
                        Id = cmd.GetInt("Id"),
                        Name = cmd.GetString("Name"),
                        Team = cmd.GetString("Team"),
                        Position = cmd.GetString("Position"),
                        Handedness = cmd.GetString("Handedness"),
                        DraftYear = cmd.GetInt("DraftYear")
                    };
                    prospects.Enqueue(dProspect);
                }
            }

            return prospects;
        }
    }
}
