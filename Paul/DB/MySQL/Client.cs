using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.DB.MySQL
{
    static class Client
    {
        public enum StatusType
        {
            ACTIVE,
            MAINTENANCE,
            DEPRECATED,
        }

        public class NodeInfo
        {
            public int No = 0;
            public StatusType Status = StatusType.MAINTENANCE;

            public string Host = null;
            public ushort Port = 0;
            public string DBName = null;

            public string User = null;
            public string Password = null;

            public ushort MinPool = 0;
            public ushort MaxPool = 0;
        }

        #region ConnectionPool
        public class ConnectionPool
        {
            private string _connectionString = null;
            private Paul.DB.MySQL.Connection.Mode _rwMode = Paul.DB.MySQL.Connection.Mode.RW;

            public ConnectionPool(string connectionString, Paul.DB.MySQL.Connection.Mode rwMode = Paul.DB.MySQL.Connection.Mode.RW)
            {
                if (false == string.IsNullOrEmpty(connectionString))
                {
                    _connectionString = connectionString;
                    _rwMode = rwMode;
                }
            }

            public Paul.DB.MySQL.Connection GetConnection()
            {
                return new Paul.DB.MySQL.Connection(_connectionString, _rwMode);
            }

        }
        #endregion ConnectionPool
        #region Replication & Sharding
        public class Replication
        {
            private ConnectionPool _master = null;
            private List<ConnectionPool> _slave = new List<ConnectionPool>();

            public ConnectionPool Master { get { return _master; } }
            public ConnectionPool Slave { get { return _slave.RandomSelect() ?? _master; } }

            public void AddMaster(Paul.DB.MySQL.ConnectionString connectionString)
            {
                // read & write
                _master = new ConnectionPool(connectionString.ToString(), Paul.DB.MySQL.Connection.Mode.RW);
            }
            public void AddSlave(Paul.DB.MySQL.ConnectionString connectionString)
            {
                // read only
                _slave.Add(new ConnectionPool(connectionString.ToString(), Paul.DB.MySQL.Connection.Mode.RO));
            }
        }
        public class Sharding
        {
            private Dictionary<int, ConnectionPool> _connections = new Dictionary<int, ConnectionPool>();

            public Paul.DB.MySQL.Connection GetConnection(ulong usrId)
            {
                var acctInfo = Paul.Cache.UserData.AccountList[usrId];
                if (null != acctInfo)
                {
                    var connection = this.GetConnection(acctInfo.MySQLNo);
                    if (null != connection) return connection;

                    Log.Error("not found user database. (usrId={0}, mysqlNo={1})", usrId, acctInfo.MySQLNo);
                    return null;
                }
                Log.Error("not found user database. (usrId={0})", usrId);
                return null;
            }
            public Paul.DB.MySQL.Connection GetConnection(int mysqlNo)
            {
                lock (_connections)
                {
                    if (true == _connections.ContainsKey(mysqlNo))
                        return _connections[mysqlNo].GetConnection();

                    var db = GetUserDatabase(mysqlNo);
                    if (null != db)
                    {
                        this.Add(db.No, new Paul.DB.MySQL.ConnectionString(
                            db.Host, db.Port, db.DBName, db.User, db.Password, db.MinPool, db.MaxPool));

                        return _connections[mysqlNo].GetConnection();
                    }
                    return null;
                }
            }
            public bool Add(int shardKey, Paul.DB.MySQL.ConnectionString connectionString)
            {
                if (true == _connections.ContainsKey(shardKey))
                    return false;

                // read & write
                _connections.Add(shardKey, new ConnectionPool(connectionString.ToString(), Paul.DB.MySQL.Connection.Mode.RW));
                return true;
            }
        }
        #endregion Replication & Sharding

        public static Replication Global = new Replication();
        public static Sharding User = new Sharding();

        public static void Initialize(ConnectionString database)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Global Database (Replication) -- Load from config file
            Global = new Replication();

            Global.AddMaster(database);

            Log.Debug("<<Config>> Database Replication[Master] : {0}", database);

        }

        private static List<Paul.DB.MySQL.Client.NodeInfo> GetUserDatabase()
        {
            using (var connection = Paul.DB.MySQL.Client.Global.Slave.GetConnection())
            {
                return Query.GetMySqlConfig.Fetch(connection);
            }
        }
        private static Paul.DB.MySQL.Client.NodeInfo GetUserDatabase(int shardKey)
        {
            using (var connection = Paul.DB.MySQL.Client.Global.Slave.GetConnection())
            {
                return Query.GetMySqlConfig.Fetch(connection, shardKey);
            }
        }
    }
}
