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



        /* *******************************************************
         * 
         */

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

        public List<DLeague> GetAllLeagues()
        {
            var command = "SELECT * FROM League";
            List<DLeague> leagues = new List<DLeague>();

            using (var cmd = Select(command))
            {
                while (cmd.Read())
                {
                    leagues.Add(InstantiateLeague(cmd));
                }
            }

            return leagues;
        }

        public DLeague GetLeague(int id)
        {
            var table = "League";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (!cmd.Read())
                {
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
                if (!cmd.Read())
                {
                    throw new Exception(String.Format("No Leagues with Name: {0}", name));
                }

                var league = InstantiateLeague(cmd);
                return league;
            }
        }


        /* *******************************************************
         * 
         */

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

        public List<DTeam> GetAllTeams()
        {
            var command = "SELECT * FROM Team";
            List<DTeam> teams = new List<DTeam>();

            using (var cmd = Select(command))
            {
                while (cmd.Read())
                {
                    teams.Add(InstantiateTeam(cmd));
                }
            }

            return teams;
        }

        public DTeam GetTeam(int id)
        {
            var table = "Team";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (!cmd.Read())
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
                if (!cmd.Read())
                {
                    throw new Exception(String.Format("No Teams with Name: {0} and LeagueId: {1}", name, leagueId));
                }

                var team = InstantiateTeam(cmd);
                return team;
            }
        }


        /* *******************************************************
         * 
         */

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
                cmd.SetInArg("@TeamId", prospect.Team.Id);
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

        public List<DProspect> GetAllProspects()
        {
            var command = "SELECT * FROM Prospect";
            List<DProspect> prospects = new List<DProspect>();

            using (var cmd = Select(command))
            {
                while (cmd.Read())
                {
                    prospects.Add(InstantiateProspect(cmd));
                }
            }

            return prospects;
        }

        public DProspect GetProspect(int id)
        {
            var table = "Prospect";
            var conditions = "Id = " + id;

            using (var cmd = Select(table, conditions))
            {
                if (!cmd.Read())
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
                if (!cmd.Read())
                {
                    throw new Exception(String.Format("No Prospects with Name: {0} and BirthDay: {1}", name, birthDay));
                }

                var prospect = InstantiateProspect(cmd);
                return prospect;
            }
        }

        public Queue<DProspect> GetProspectsByYearAsQueue(int year)
        {
            var prospects = new Queue<DProspect>();

            var table = "Prospect";
            var conditions = "DraftYear = " + year;

            using (var cmd = Select(table, conditions))
            {
                while (cmd.Read())
                {
                    prospects.Enqueue(InstantiateProspect(cmd));
                }
            }

            return prospects;
        }

        /* *******************************************************
         * 
         */

        private SqlCmdExt Select(string table, string conditions)
        {
            var command = "SELECT * FROM " + table + " WHERE " + conditions;

            return Select(command);
        }

        private SqlCmdExt Select(string command)
        {
            var cmd = new SqlCmdExt(_connectionString);

            cmd.CreateCmd(command);

            cmd.ExecuteSelect();

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
    }
}
