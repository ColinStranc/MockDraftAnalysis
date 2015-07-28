using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DatabaseCache
    {
        private Dictionary<int, string> _idToHandedness;
        private Dictionary<int, string> _idToPosition;
        private Dictionary<string, int> _handednessToId;
        private Dictionary<string, int> _positionToId;

        private readonly string _connectionString;

        public DatabaseCache(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetHandedness(int id)
        {
            if (_idToHandedness == null)
            {
                LoadHandednessPairs();
            }

            return _idToHandedness[id];
        }

        public int GetHandednessId(string hand)
        {
            if (_handednessToId == null)
            {
                LoadHandednessPairs();
            }

            return _handednessToId[hand];
        }

        public string GetPosition(int id)
        {
            if (_idToPosition == null)
            {
                LoadPositionPairs();
            }

            return _idToPosition[id];
        }

        public int GetPositionId(string position)
        {
            if (_positionToId == null)
            {
                LoadPositionPairs();
            }

            return _positionToId[position];
        }

        /* *******************************************************
         * 
         */

        private void LoadPositionPairs()
        {
            _idToPosition = new Dictionary<int, string>();
            _positionToId = new Dictionary<string, int>();

            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id, Position
                    FROM
                        Position
                ");

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    var id = cmd.GetInt("Id");
                    var position = cmd.GetString("Position");

                    _idToPosition.Add(id, position);
                    _positionToId.Add(position, id);
                }
            }
        }

        private void LoadHandednessPairs()
        {
            _idToHandedness = new Dictionary<int, string>();
            _handednessToId = new Dictionary<string, int>();

            using (var cmd = new SqlCmdExt(_connectionString))
            {
                cmd.CreateCmd(@"
                    SELECT
                        Id, Hand
                    FROM
                        Handedness
                ");

                cmd.ExecuteSelect();

                while (cmd.Read())
                {
                    var id = cmd.GetInt("Id");
                    var hand = cmd.GetString("Hand");

                    _idToHandedness.Add(id, hand);
                    _handednessToId.Add(hand, id);
                }
            }
        }
    }
}
