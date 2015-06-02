using System;
using System.Text;

namespace Paul.DB.MySQL
{
    public class ConnectionString
    {
        private string _connectionString = null;

        // reference: http://www.connectionstrings.com/mysql/
        public ConnectionString(string host, ushort port, string database, string usrId, string usrPwd, ushort minPoolSize = 0, ushort maxPoolSize = 0)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendFormat("server={0};", host);
            sb.AppendFormat("port={0};", port);
            sb.AppendFormat("database={0};", database);
            sb.AppendFormat("user={0};", usrId);
            sb.AppendFormat("password={0};", usrPwd);
            sb.AppendFormat("charset={0};", "utf8");
            sb.AppendFormat("encrypt={0};", true);			// Using encryption (old)

            sb.AppendFormat("Connection Timeout={0};", 10);
            sb.AppendFormat("Connection Reset={0};", true);
            sb.AppendFormat("Connection Lifetime={0};", 300);		// Recycle connections in pool

            sb.AppendFormat("default command timeout={0};", 30);
            sb.AppendFormat("allow zero datetime={0};", true);

            sb.AppendFormat("Allow User Variables={0};", true);

            if (0 < maxPoolSize)
            {
                sb.AppendFormat("Pooling={0};", true);
                sb.AppendFormat("Min Pool Size={0};", minPoolSize);
                sb.AppendFormat("Max Pool Size={0};", maxPoolSize);
            }

            _connectionString = sb.ToString();
        }

        public override string ToString()
        {
            return _connectionString;
        }
    }
}
