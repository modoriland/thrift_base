using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.Cache.User
{
    class UserInfo
    {
        public ulong userSerial = 0;
        public int userLv = 0;
        public string userName = "";

        public AvtInfo _avtInfo = new AvtInfo();
    }
    class User
    {
        private Dictionary<ulong, UserInfo> _userList = new Dictionary<ulong, UserInfo>();

        public Paul.Cache.User.UserInfo Refresh(ulong userSerial, bool force = true)
        {
            Paul.Cache.User.UserInfo userInfo = null;
            lock(_userList)
            {
                if (_userList.ContainsKey(userSerial))
                    userInfo = _userList[userSerial];
            }

            if (null == userInfo || force)
            {
                // db 에서 읽어오기
                using (var connection = Paul.DB.MySQL.Client.User.GetConnection(userSerial))
                {
                    userInfo = Paul.DB.MySQL.Query.UserInfo.GetUserInfo(connection, userSerial);
                    if (null != userInfo)
                    {
                        lock (_userList)
                        {
                            if (false == _userList.ContainsKey(userInfo.userSerial))
                            {
                                _userList.Add(userSerial, userInfo);
                            }
                        }
                    }
                    else
                        Log.Error("Paul.Cache.User.UserInfo Refresh : {0}", userSerial);
                }
            }

            return userInfo;
        }

        public Paul.Cache.User.UserInfo Get(ulong userSerial)
        {
            return Refresh(userSerial, false);
        }
    }
}
