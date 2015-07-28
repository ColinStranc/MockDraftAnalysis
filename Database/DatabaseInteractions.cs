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
        private static Dictionary<string, DatabaseCache> _caches = new Dictionary<string, DatabaseCache>();
        private DatabaseCache _cache;

        public DatabaseInteractions(string connectionString) 
        {
            _connectionString = connectionString;
            
            if (!_caches.ContainsKey(connectionString))
            {
                _caches.Add(connectionString, new DatabaseCache(connectionString));
            }
            _cache = _caches[_connectionString];
        }

        /* *******************************************************
         * 
         */

        public int GetHandednessId(string hand)
        {
            return _cache.GetHandednessId(hand);
        }

        public string GetHandedness(int id)
        {
            return _cache.GetHandedness(id);
        }

        public int GetPositionId(string position)
        {
            return _cache.GetPositionId(position);
        }

        public string GetPosition(int id)
        {
            return _cache.GetPosition(id);
        }


        

        public DLeague GetLeague(int id)
        {
            var table = "League";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    //throw new NoRowsMeetConditionsException("Id", id);
                    throw new Exception(String.Format("No Leagues with Id: {0}", id));
                }

                var league = InstantiateLeague(cmd);
                return league;
            }
        }

        public DLeague GetLeague(string name)
        {
            var table = "League";
            var conditions = "Name = '" + name + "'";

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    throw new Exception(String.Format("No Leagues with Name: {0}", name));
                }

                var league = InstantiateLeague(cmd);
                return league;
            }
        }

        public DTeam GetTeam(int id)
        {
            var table = "Team";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    throw new Exception(String.Format("No Teams with Id: {0}", id));
                }

                var team = InstantiateTeam(cmd);
                return team;
            }
        }

        public DTeam GetTeam(string name, int leagueId)
        {
            var table = "Team";
            var conditions = "Name = '" + name + "' AND LeagueId = " + leagueId;

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    throw new Exception(String.Format("No Teams with Name: {0} and LeagueId: {1}", name, leagueId));
                }

                var team = InstantiateTeam(cmd);
                return team;
            }
        }

        public DProspect GetProspect(int id)
        {
            var table = "Prospect";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    throw new Exception(String.Format("No Prospects with Id: {0}", id));
                }

                var prospect = InstantiateProspect(cmd);
                return prospect;
            }
        }

        public DProspect GetProspect(string name, DateTime birthDay)
        {
            var table = "Prospect";
            var conditions = "Name = '" + name + "' AND BirthDay = " + birthDay;

            using (var cmd = Select(table, conditions))
            {
                if (cmd == null)
                {
                    throw new Exception(String.Format("No Prospects with Name: {0} and BirthDay: {1}", name, birthDay));
                }

                var prospect = InstantiateProspect(cmd);
                return prospect;
            }
        }

        /* *******************************************************
         * 
         */

        private SqlCmdExt Select(string table, string conditions)
        {
            var cmd = new SqlCmdExt(_connectionString);
            
            var command = "SELECT * FROM " + table + " WHERE " + conditions;
            cmd.CreateCmd(command);

            cmd.ExecuteSelect();

            if (!cmd.Read()) return null;

            return cmd;
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

        /* *******************************************************
         * 
         */



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

            return GetLeague(leagueId);
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
