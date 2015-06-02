using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.DB.MySQL.Query
{
    using StatusType = Paul.DB.MySQL.Client.StatusType;

    static class GetMySqlConfig
    {
        private static Func<Paul.DB.MySQL.ResultSet, Paul.DB.MySQL.Client.NodeInfo> __Fetch = (row) =>
        {
            var db = new Paul.DB.MySQL.Client.NodeInfo();

            db.No = row.GetInt32("no") ?? 0;
            db.Status = row.GetEnum<StatusType>("status") ?? StatusType.MAINTENANCE;

            db.Host = row.GetString("ip");
            db.Port = row.GetUInt16("port") ?? 0;
            db.DBName = row.GetString("db");

            db.User = row.GetString("user");
            db.Password = row.GetString("passwd");

            db.MinPool = row.GetUInt16("min_pool") ?? 8;
            db.MaxPool = row.GetUInt16("max_pool") ?? 16;

            return db;
        };

        public static List<Paul.DB.MySQL.Client.NodeInfo> Fetch(Paul.DB.MySQL.Connection connection)
        {
            const string queryString = "SELECT no, status, ip, port, db, user, passwd, min_pool, max_pool FROM shard_db";

            using (var query = new Paul.DB.MySQL.DirectQuery(connection, queryString, Paul.DB.MySQL.Connection.Mode.RO))
            {
                return new List<Paul.DB.MySQL.Client.NodeInfo>(query.FetchAll(__Fetch));
            }
        }

        public static Paul.DB.MySQL.Client.NodeInfo Fetch(Paul.DB.MySQL.Connection connection, int no)
        {
            const string queryString = "SELECT no, status, ip, port, db, user, passwd, min_pool, max_pool FROM shard_db WHERE no = @no";

            using (var query = new Paul.DB.MySQL.DirectQuery(connection, queryString, Paul.DB.MySQL.Connection.Mode.RO))
            {
                query.AddParam("@no", no);

                return query.Fetch(__Fetch);
            }
        }
    }
	
}
