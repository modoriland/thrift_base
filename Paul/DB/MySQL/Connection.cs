using System;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Paul.DB.MySQL
{
    public class Connection : IDisposable
    {
        public enum Mode
        {
            RW = 0,	// read & write
            RO = 1,	// read only
        }
        private MySqlConnection _mysqlConnection = null;
        private MySqlTransaction _mysqlTransaction = null;
        private Mode _mode = Mode.RW;

        public Connection(string connectionString, Mode mode = Mode.RW)
        {
            _mode = mode;
            _mysqlConnection = new MySqlConnection(connectionString);

            // $TODO: 접속하는데 걸리는 시간을 tracking 하자.
            _mysqlConnection.Open();
        }

        #region Finalizer & Dispose
        ~Connection()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (true == disposing)
            {
                MySqlConnection connection = null;
                connection = Interlocked.Exchange(ref _mysqlConnection, connection);
                if (null != connection && System.Data.ConnectionState.Open == connection.State)
                {
                    Rollback();
                    connection.Close();
                }
            }
        }
        #endregion Finalizer & Dispose

        public MySqlConnection GetConnection() { return _mysqlConnection; }
        public MySqlTransaction GetTransaction() { return _mysqlTransaction; }

        public bool IsReadOnly { get { return Mode.RO == _mode; } }
        public System.Data.ConnectionState State
        {
            get
            {
                return (null != _mysqlConnection) ? _mysqlConnection.State : System.Data.ConnectionState.Closed;
            }
        }

        // IsolationLevel: http://msdn.microsoft.com/ko-kr/library/system.transactions.isolationlevel.aspx
        public void BeginTransaction(System.Data.IsolationLevel iso = System.Data.IsolationLevel.ReadCommitted)
        {
            if (null != _mysqlConnection)
                _mysqlTransaction = _mysqlConnection.BeginTransaction(iso);
        }
        public void Commit()						// implementation for IDBConnection 
        {
            MySqlTransaction transaction = null;
            transaction = Interlocked.Exchange(ref _mysqlTransaction, transaction);

            if (null != transaction)
                transaction.Commit();
        }
        public void Rollback()						// implementation for IDBConnection 
        {
            MySqlTransaction transaction = null;
            transaction = Interlocked.Exchange(ref _mysqlTransaction, transaction);

            if (null != transaction)
                transaction.Rollback();
        }
    }

    public struct Transaction : IDisposable
    {
        private Connection _connection;
        public Transaction(Connection connection)
        {
            _connection = connection;
            if (null != connection && System.Data.ConnectionState.Open == connection.State)
                connection.BeginTransaction();
        }
        public void Dispose()
        {
            Rollback();
        }
        public void Commit()
        {
            var connection = _connection;
            if (null != connection && System.Data.ConnectionState.Open == connection.State)
                connection.Commit();
        }
        public void Rollback()
        {
            Connection connection = null;
            connection = Interlocked.Exchange(ref _connection, connection);
            if (null != connection && System.Data.ConnectionState.Open == connection.State)
                connection.Rollback();
        }
    }
}
