using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul
{
    public class Server : Service.Iface
    {
        private DB.MySQL.ConnectionString _connectionString =
            new DB.MySQL.ConnectionString("localhost", 3306, "dbase","paul","pw");
        public Server() { }

        public RplUserLogin ulogin(ReqUserLogin userData)
        { 
            RplUserLogin ret = new RplUserLogin();

            var userInfo = Paul.Cache.UserData.UserList.Get((ulong)userData.UserSerial);

            if (null != userInfo)
            {
                ret.UserSerial = (long)userInfo.userSerial;
                ret.Id = userInfo.userName;
                ret.Result = ResultUserLogin.Success;
            }

            return ret;
        }

        public List<Work> checkcalc(int a, int b)
        {
            StringBuilder sb = new StringBuilder();
            List<Work> list = new List<Work>();
            Work data = new Work();
            
            sb.AppendFormat("add : {0}", a + b);
            SetworkData(ref data, a, b, sb.ToString());
            list.Add(data);
            consoleWrite(sb.ToString());

            data = new Work();
            sb.Clear();
            sb.AppendFormat("sub : {0}", a - b);
            SetworkData(ref data, a, b, sb.ToString());
            list.Add(data);
            consoleWrite(sb.ToString());

            data = new Work();
            sb.Clear();
            sb.AppendFormat("mul : {0}", a * b);
            SetworkData(ref data, a, b, sb.ToString());
            list.Add(data);
            consoleWrite(sb.ToString());

            data = new Work();
            sb.Clear();
            sb.AppendFormat("div : {0}", a / b);
            SetworkData(ref data, a, b, sb.ToString());
            list.Add(data);
            consoleWrite(sb.ToString());

            data = new Work();
            sb.Clear();
            sb.AppendFormat("mod : {0}", a % b);
            SetworkData(ref data, a, b, sb.ToString());
            list.Add(data);
            consoleWrite(sb.ToString());

            data = new Work();
            sb.Clear();
            sb.AppendFormat("list count : {0}", list.Count());
            consoleWrite(sb.ToString());

            return list;
        }

        private void consoleWrite(string str)
        {
            Console.WriteLine(str.ToString());
        }

        private void SetworkData(ref Work data, int a, int b, string str)
        {
            data.Num1 = a;
            data.Num2 = b;
            data.Comment = str.ToString();
        }
    }
}
