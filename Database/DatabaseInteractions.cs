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

        public void AddLeague(DLeague league)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    INSERT INTO League (Name)
                    VALUES (@Name)
                ");
                cmd.SetInArg("@Name", league.Name);

                cmd.ExecuteInsertUpdateDelete();
            }
        }

        public void AddTeam(DTeam team)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    INSERT INTO Team (Name, LeagueId)
                    VALUES (@Name, @LeagueId)
                ");
                cmd.SetInArg("@Name", team.Name);
                cmd.SetInArg("@LeagueId", team.League.Id);

                cmd.ExecuteInsertUpdateDelete();
            }
        }

        public DLeague GetLeague(string leagueName)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        League
                    WHERE
                        Name = @Name
                ");
                cmd.SetInArg("@Name", leagueName);

                cmd.ExecuteSelect();

                if (!cmd.Read()) return null;

                return InstantiateLeague(cmd);
            }
        }

        public DTeam GetTeam(string teamName, int leagueId)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Team
                    WHERE
                        Name = @Name AND
                        LeagueId = @LeagueId
                ");
                cmd.SetInArg("@Name", teamName);
                cmd.SetInArg("@LeagueId", leagueId);

                cmd.ExecuteSelect();

                if (!cmd.Read()) return null;

                return InstantiateTeam(cmd);
            }
        }

        public List<DLeague> GetAllLeagues()
        {
            var leagues = new List<DLeague>();

            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        League
                ");

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    leagues.Add(InstantiateLeague(cmd));
                }
            }

            return leagues;
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

        private DTeam InstantiateTeam(SqlCmdExt cmd)
        {
            var team = new DTeam()
            {
                Id = cmd.GetInt("Id"),
                Name = cmd.GetString("Name"),
                League = GetLeague(cmd.GetInt("LeagueId"))
            };

            return team;
        }

        private DLeague GetLeague(int leagueId)
        {
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

                return InstantiateLeague(cmd);
            }
        }

        private DLeague GetLeague(int leagueId, ref List<DLeague> createdLeagues)
        {
            foreach (var league in createdLeagues)
            {
                if (league.Id == leagueId) return league;
            }

            return GetLeague(leagueId);
            /*
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
            */
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
