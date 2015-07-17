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
                    //var league = GetLeague(cmd, ref leagues);
                    //var team = GetTeam(cmd, ref teams, league);
                    
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
                    prospects.Enqueue(dProspect);
                }
            }

            return prospects;
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

                returnTeam = new DTeam()
                {
                    Id = teamId,
                    Name = cmd.GetString("Name"),
                    League = GetLeague(cmd.GetInt("LeagueId"), ref createdLeagues)
                };

                createdTeams.Add(returnTeam);
            }

            return returnTeam;
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

                returnLeague = new DLeague()
                {
                    Id = leagueId,
                    Name = cmd.GetString("Name")
                };

                createdLeagues.Add(returnLeague);
            }

            return returnLeague;
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
