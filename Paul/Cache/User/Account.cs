using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.Cache.User
{
    class AccountInfo
    {
        public ulong userSerial = 0;
        public int MySQLNo = -1;
        //public int RedisNo = -1;
        public UserStatusType Status = UserStatusType.BLOCKED;
        public long ExpireTicks = 0;

        // $TODO: 디바이스 ID 리스트가 필요하다.
    }

    class Account
    {
        private Dictionary<ulong, Paul.Cache.User.AccountInfo> _usrList = new Dictionary<ulong, Paul.Cache.User.AccountInfo>();

        private Paul.Cache.User.AccountInfo _Fill(ulong userSerial)
        {
            Paul.Cache.User.AccountInfo acctInfo = null;
            lock (_usrList)
            {
                if (true == _usrList.ContainsKey(userSerial))
                    acctInfo = _usrList[userSerial];
            }

            // $TODO:
            //		1. check 와 insert 사이의 시간 차로 인하여, 동기화 문제가 발생 할 수 있다. 수정할 것..
            //		2. master 와 slave 간의 동기화 시간차로 인하여 문제가 발생할 수 있다.

            if (null == acctInfo)
            {
                var connection = Paul.DB.MySQL.Client.Global.Slave.GetConnection();
                acctInfo = Paul.DB.MySQL.Query.Account.GetAccount(connection, userSerial);

                if (null != acctInfo)
                {
                    lock (_usrList)
                    {
                        if (false == _usrList.ContainsKey(acctInfo.userSerial))
                        {
                            acctInfo.ExpireTicks = DateTime.Now.Ticks + (300 * TimeSpan.TicksPerSecond);
                            _usrList.Add(userSerial, acctInfo);
                        }
                    }
                }
                else
                    Log.Error("Paul.Cache.User.AccountInfo _Fill");
            }

            return acctInfo;
        }

        public Paul.Cache.User.AccountInfo this[ulong userSerial]
        {
            get
            {
                return _Fill(userSerial);
            }
        }

        public void FlushExpiredData()
        {
            List<ulong> expiredList = null;
            lock (_usrList)
            {
                expiredList = _usrList
                    .Where(elem => elem.Value.ExpireTicks <= DateTime.Now.Ticks)
                    .Select(elem => elem.Key)
                    .ToList();
            }

            foreach (var userSerial in expiredList)
            {
                var info = _Fill(userSerial);
                lock (_usrList)
                {
                    _usrList.Remove(userSerial);
                }
            }
        }
    }
}
