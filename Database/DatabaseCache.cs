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
        private object _locker = new object();

        private readonly string _connectionString;

        public DatabaseCache(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetHandedness(int id)
        {
            lock (_locker)
            {
                if (_idToHandedness == null)
                {
                    LoadHandednessPairs();
                }

                return _idToHandedness[id];
            }
        }

        public int GetHandednessId(string hand)
        {
            lock (_locker)
            {
                if (_handednessToId == null)
                {
                    LoadHandednessPairs();
                }

                return _handednessToId[hand];
            }
        }

        public string GetPosition(int id)
        {
            lock (_locker)
            {
                if (_idToPosition == null)
                {
                    LoadPositionPairs();
                }

                return _idToPosition[id];
            }
        }

        public int GetPositionId(string position)
        {
            lock (_locker)
            {
                if (_positionToId == null)
                {
                    LoadPositionPairs();
                }

                return _positionToId[position];
            }
        }

        /* *******************************************************
         * 
         */

        private void LoadPositionPairs()
        {
            var idToPosition = new Dictionary<int, string>();
            var positionToId = new Dictionary<string, int>();

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

                    idToPosition.Add(id, position);
                    positionToId.Add(position, id);
                }
            }

            _idToPosition = idToPosition;
            _positionToId = positionToId;
        }

        private void LoadHandednessPairs()
        {
            var idToHandedness = new Dictionary<int, string>();
            var handednessToId = new Dictionary<string, int>();

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

                    idToHandedness.Add(id, hand);
                    handednessToId.Add(hand, id);
                }
            }

            _idToHandedness = idToHandedness;
            _handednessToId = handednessToId;
        }
    }
}
