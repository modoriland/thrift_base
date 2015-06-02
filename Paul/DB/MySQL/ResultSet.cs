using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Paul.DB.MySQL
{
    public class ResultSet : IDisposable
    {
        private MySqlDataReader _mysqlDataReader = null;

        public ResultSet(MySqlDataReader reader)
        {
            _mysqlDataReader = reader;
        }

        #region Finalizer & Dispose
        ~ResultSet()
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
                MySqlDataReader reader = null;
                reader = Interlocked.Exchange(ref _mysqlDataReader, reader);
                if (null != reader)
                    reader.Dispose();
            }
        }
        #endregion Finalizer & Dispose

        public object GetValue(string column)
        {
            if (null != _mysqlDataReader && false == string.IsNullOrEmpty(column))
            {
                var value = _mysqlDataReader[column];
                if (null != value && value != System.DBNull.Value)
                    return value;
                return null;
            }

            //////////////////////////////////////////////////////////////////////////////

            if (null == _mysqlDataReader)
                throw new System.NullReferenceException(
                    "invalid attempt to call GetValue() when Motors.MySql.ResultSet is disposed.");
            else
                throw new System.ArgumentException(string.Format("invalid column='{0}'", column));
        }

        public bool Next()
        {
            return _mysqlDataReader.Read();
        }
    }

    public class OutParam
    {
        public OutParam(MySqlParameter outParam)
        {
            if (null != outParam)
                this.Value = outParam.Value;
            else
                this.Value = null;
        }

        public object Value { get; private set; }

        public new string ToString()
        {
            if (null != Value)
                return System.Convert.ToString(Value);
            return null;
        }
    }
}
