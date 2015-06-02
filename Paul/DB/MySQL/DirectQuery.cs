using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Linq;


namespace Paul.DB.MySQL
{
    public class DirectQuery : Command
    {
        public DirectQuery(Connection connection, string queryString, Connection.Mode mode = Connection.Mode.RW)
        {
            if (connection.IsReadOnly && Connection.Mode.RW == mode)
            {
                Log.Fatal("cannot use Motors.MySql.Connection for writing: connection is read-only\n{0}", Log.StackInfo(0));
            }

            _mysqlCommand.CommandType = System.Data.CommandType.Text;
            _mysqlCommand.CommandText = queryString;
            _mysqlCommand.Connection = connection.GetConnection();
            _mysqlCommand.Transaction = connection.GetTransaction();
            _mysqlCommand.Prepare();
        }
        public IEnumerable<T> FetchAll<T>(Func<ResultSet, T> fetcher)
        {
            //System.Console.Error.WriteLine("Fetch()");
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                while (resultSet.Next())
                    yield return fetcher(resultSet);
            }
        }
        public T Fetch<T>(Func<ResultSet, T> fetcher)
        {
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                if (resultSet.Next())
                    return fetcher(resultSet);
                return default(T);
            }
        }
        public void FetchAll(Action<ResultSet> fetcher)
        {
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                while (resultSet.Next())
                    fetcher(resultSet);
            }
        }
        public void Fetch(Action<ResultSet> fetcher)
        {
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                if (resultSet.Next())
                    fetcher(resultSet);
            }
        }
    }
}
