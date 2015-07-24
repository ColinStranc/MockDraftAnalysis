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

        public void AddProspect(DProspect prospect)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    INSERT INTO Prospect
                    (
                        Name,
                        TeamId,
                        Height,
                        Weight,
                        PositionId,
                        HandednessId,
                        BirthDay,
                        DraftYear,
                        BirthCity,
                        BirthCountry,
                        Notes
                    )
                    VALUES
                    (
                        @Name,
                        @TeamId,
                        @Height,
                        @Weight,
                        @PositionId,
                        @HandednessId,
                        @BirthDay,
                        @DraftYear,
                        @BirthCity,
                        @BirthCountry,
                        @Notes
                    )");
                cmd.SetInArg("@Name", prospect.Name);
                cmd.SetInArg("@TeamId", GetTeamId(prospect.Team));
                cmd.SetInArg("@Height", prospect.Height);
                cmd.SetInArg("@Weight", prospect.Weight);
                cmd.SetInArg("@PositionId", GetPositionId(prospect.Position));
                cmd.SetInArg("@HandednessId", GetHandednessId(prospect.Handedness));
                cmd.SetInArg("@BirthDay", prospect.BirthDay);
                cmd.SetInArg("@DraftYear", prospect.DraftYear);
                cmd.SetInArg("@BirthCity", prospect.BirthCity);
                cmd.SetInArg("@BirthCountry", prospect.BirthCountry);
                cmd.SetInArg("@Notes", prospect.Notes);

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

        public DProspect GetProspect(string name, string position, int teamId)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Prospect
                    WHERE
                        Name = @Name AND
                        PositionId = @PositionId AND
                        TeamId = @TeamId
                    ");
                cmd.SetInArg("@Name", name);
                cmd.SetInArg("@PositionId", GetPositionId(position));
                cmd.SetInArg("@TeamId", teamId);

                cmd.ExecuteSelect();

                if (!cmd.Read()) return null;

                return InstantiateProspect(cmd);
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

        public List<DTeam> GetAllTeams()
        {
            var teams = new List<DTeam>();

            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        *
                    FROM
                        Team
                ");

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    teams.Add(InstantiateTeam(cmd));
                }
            }

            return teams;
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

        private DProspect InstantiateProspect(SqlCmdExt cmd)
        {
            var dProspect = new DProspect()
            {
                Id = cmd.GetInt("Id"),
                Name = cmd.GetString("Name"),
                Team = GetTeam(cmd.GetInt("TeamId")),
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

        private DTeam GetTeam(int teamId)
        {
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

                return InstantiateTeam(cmd);
            }
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

        private int GetPositionId(string position)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id
                    FROM
                        Position
                    WHERE
                        Position = @Position
                ");
                cmd.SetInArg("@Position", position);

                cmd.ExecuteSelect();
                if (!cmd.Read()) return -1;

                return cmd.GetInt("Id");
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

        private int GetHandednessId(string handedness)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id
                    FROM
                        Handedness
                    WHERE
                        Hand = @Hand
                ");
                cmd.SetInArg("@Hand", handedness);

                cmd.ExecuteSelect();
                if (!cmd.Read()) return -1;

                return cmd.GetInt("Id");
            }
        }

        private int GetTeamId(DTeam team)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id
                    FROM
                        Team
                    WHERE
                        Name = @Name AND
                        LeagueId = @LeagueId
                ");
                cmd.SetInArg("@Name", team.Name);
                cmd.SetInArg("@LeagueId", GetLeagueId(team.League));

                cmd.ExecuteSelect();
                if (!cmd.Read()) return -1;

                return cmd.GetInt("Id");
            }
        }

        private int GetLeagueId(DLeague league)
        {
            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id
                    FROM
                        League
                    WHERE
                        Name = @Name
                ");
                cmd.SetInArg("@Name", league.Name);

                cmd.ExecuteSelect();
                if (!cmd.Read()) return -1;

                return cmd.GetInt("Id");
            }
        }
    }
}
