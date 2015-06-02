using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.DB.MySQL.Query
{
    static class UserInfo
    {
        private static Func<Paul.DB.MySQL.ResultSet, Paul.Cache.User.UserInfo> __Fetch = (row) =>
        {
            var usrInfo = new Paul.Cache.User.UserInfo();

            usrInfo.userSerial = row.GetUInt64("userSerial") ?? 0;
            usrInfo.userName = row.GetString("name") ?? "";
            usrInfo.userLv = row.GetInt32("portrait") ?? 0;

            return usrInfo;
        };

        public static Paul.Cache.User.UserInfo GetUserInfo(Paul.DB.MySQL.Connection connection, ulong usrId)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_usr_get_user_info"))
            {
                query.AddParam("p_usrId", usrId);

                return query.Fetch(__Fetch);
            }
        }

    }
}
