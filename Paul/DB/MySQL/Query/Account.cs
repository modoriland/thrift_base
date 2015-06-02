using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


namespace Paul.DB.MySQL.Query
{
    static class Account
    {
        public static ulong CreateAccount(Paul.DB.MySQL.Connection connection, string id, string pwd, string ip)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_create_account_basic", Paul.DB.MySQL.Connection.Mode.RW))
            {
                query.AddParam("p_id", id);
                query.AddParam("p_pwd", pwd);
                query.AddParam("p_ip", ip);

                return query.Fetch((row) => { return row.GetUInt64("usrId") ?? 0; });
            }
        }

        public static ulong CreateAccount(Paul.DB.MySQL.Connection connection, long kakaoId, string hashedTalkId, string ip)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_create_account_kakao", Paul.DB.MySQL.Connection.Mode.RW))
            {
                query.AddParam("p_kakao_id", kakaoId);
                query.AddParam("p_hashed_talk_id", hashedTalkId);
                query.AddParam("p_ip", ip);

                return query.Fetch((row) => { return row.GetUInt64("usrId") ?? 0; });
            }
        }

        public static Paul.Cache.User.AccountInfo GetAccount(Paul.DB.MySQL.Connection connection, ulong userSerial)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_get_account", Paul.DB.MySQL.Connection.Mode.RO))
            {
                query.AddParam("p_userserial", userSerial);

                return query.Fetch((row) =>
                {
                    var acctInfo = new Paul.Cache.User.AccountInfo();
                    acctInfo.userSerial = userSerial;
                    acctInfo.MySQLNo = row.GetInt32("mysql_no") ?? -1;
                    //acctInfo.RedisNo = row.GetInt32("redis_no") ?? -1;
                    acctInfo.Status = row.GetEnum<UserStatusType>("status") ?? 0;
                    return acctInfo;
                });
            }
            // $TODO: 디바이스 ID 리스트가 필요하다.
        }

        public static ulong Login(Paul.DB.MySQL.Connection connection, string id, string pwd, string ip, ref ErrorCode errCode)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_login_basic", Paul.DB.MySQL.Connection.Mode.RW))
            {
                query.AddParam("p_id", id);
                query.AddParam("p_pwd", pwd);
                query.AddParam("p_ip", ip);

                query.SetOutParam("p_retcode", MySqlDbType.Enum);

                ulong userSerial = 0;
                var status = UserStatusType.MAINTENANCE;

                query.Fetch((row) =>
                {
                    userSerial = row.GetUInt64("userSerial") ?? 0;
                    status = row.GetEnum<UserStatusType>("status") ?? 0;
                });

                errCode = query.GetOutParam("p_retcode").ToEnum<ErrorCode>();
                if (0 != userSerial && ErrorCode.E_SUCCESS == errCode)
                    return userSerial;

                return 0;
            }
        }

        public static ulong Login(Paul.DB.MySQL.Connection connection, long kakaoId, string talkId, string ip, ref ErrorCode errCode)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_login_kakao", Paul.DB.MySQL.Connection.Mode.RO))
            {
                query.AddParam("p_kakao_id", kakaoId);
                query.AddParam("p_hashed_talk_Id", talkId);
                query.AddParam("p_ip", ip);

                query.SetOutParam("p_retcode", MySqlDbType.Enum);

                ulong userSerial = 0;
                var status = UserStatusType.MAINTENANCE;

                query.Fetch((row) =>
                {
                    userSerial = row.GetUInt64("userSerial") ?? 0;
                    status = row.GetEnum<UserStatusType>("status") ?? 0;
                });

                errCode = query.GetOutParam("p_retcode").ToEnum<ErrorCode>();
                if (0 != userSerial && ErrorCode.E_SUCCESS == errCode)
                    return userSerial;

                return 0;
            }
        }

        public static void Logout(Paul.DB.MySQL.Connection connection, ulong userSerial)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_logout", Paul.DB.MySQL.Connection.Mode.RW))
            {
                query.AddParam("p_userserial", userSerial);
                query.Execute();
            }
        }

        public static void Leave(Paul.DB.MySQL.Connection connection, ulong userSerial)
        {
            using (var query = new Paul.DB.MySQL.Procedure(connection, "sp_act_leave", Paul.DB.MySQL.Connection.Mode.RW))
            {
                query.AddParam("p_userserial", userSerial);
                query.Execute();
            }
        }
    }
}
