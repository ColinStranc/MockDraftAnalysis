using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseModels;

namespace Database
{
    public class DatabaseInteractions
    {
        private readonly string _connectionString;

        public DatabaseInteractions(string connectionString) 
        {
            _connectionString = connectionString;
        }

        public Queue<DProspect> GetProspectsByYearAsQueue(int year)
        {
            Queue<DProspect> prospects = new Queue<DProspect>();
            List<DTeam> teams = new List<DTeam>();
            List<DLeague> leagues = new List<DLeague>();

            using (var cmd = new SqlCmdExt(_connectionString))
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
                    var dProspect = InstantiateProspect(cmd, ref teams, ref leagues);
                    
                    prospects.Enqueue(dProspect);
                }
            }

            return prospects;
        }

        private DProspect InstantiateProspect(SqlCmdExt cmd, ref List<DTeam> teams, ref List<DLeague> leagues)
        {
            var dProspect = new DProspect()
            {
                Id = cmd.GetInt("Id"),
                Name = cmd.GetString("Name"),
                Team = GetTeam(cmd.GetInt("TeamId"), ref teams, ref leagues),
                Height = cmd.GetInt("Height"),
                Weight = cmd.GetInt("Weight"),
                Position = GetPosition(cmd.GetInt("PositionId")),
                Handedness = GetHandedness(cmd.GetInt("HandednessId")),
                BirthDay = cmd.GetDateTime("BirthDay"),
                DraftYear = cmd.GetInt("DraftYear"),
                BirthCity = cmd.GetString("BirthCity"),
                BirthCountry = cmd.GetString("BirthCountry"),
                Notes = cmd.GetString("Notes")
            };

            return dProspect;
        }

        private DTeam GetTeam(int teamId, ref List<DTeam> createdTeams, ref List<DLeague> createdLeagues)
        {
            foreach (var team in createdTeams)
            {
                if (team.Id == teamId) return team;
            }

            DTeam returnTeam;
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Team
                    WHERE
                        Id = @Id
                    ");
                cmd.SetInArg("@Id", teamId);

                cmd.ExecuteSelect();

                if (!cmd.Read()) return null;

                returnTeam = InstantiateTeam(cmd, ref createdLeagues);

                createdTeams.Add(returnTeam);
            }

            return returnTeam;
        }

        private DTeam InstantiateTeam(SqlCmdExt cmd, ref List<DLeague> createdLeagues)
        {
            var team = new DTeam()
            {
                Id = cmd.GetInt("Id"),
                Name = cmd.GetString("Name"),
                League = GetLeague(cmd.GetInt("LeagueId"), ref createdLeagues)
            };
            return team;
        }

        private DLeague GetLeague(int leagueId, ref List<DLeague> createdLeagues)
        {
            foreach (var league in createdLeagues)
            {
                if (league.Id == leagueId) return league;
            }

            DLeague returnLeague;

            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        League
                    WHERE
                        Id = @Id
                    ");
                cmd.SetInArg("@Id", leagueId);

                cmd.ExecuteSelect();

                if (!cmd.Read()) return null;

                returnLeague = InstantiateLeague(cmd);

                createdLeagues.Add(returnLeague);
            }

            return returnLeague;
        }

        private DLeague InstantiateLeague(SqlCmdExt cmd)
        {
            var league = new DLeague()
            {
                Id = cmd.GetInt("Id"),
                Name = cmd.GetString("Name")
            };
            return league;
        }

        private string GetPosition(int positionId)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Position
                    FROM
                        Position
                    WHERE
                        Id = @Id
                    ");
                cmd.SetInArg("@Id", positionId);

                cmd.ExecuteSelect();
                if (!cmd.Read()) return null;

                return cmd.GetString("Position");
            }
        }

        private string GetHandedness(int handednessId)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Hand
                    FROM
                        Handedness
                    WHERE
                        Id = @Id
                    ");
                cmd.SetInArg("@Id", handednessId);

                cmd.ExecuteSelect();
                if (!cmd.Read()) return null;

                return cmd.GetString("Hand");
            }
        }
    }
}
