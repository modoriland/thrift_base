using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Linq;

namespace Paul.DB.MySQL
{
    public abstract class Command : IDisposable
    {
        protected MySqlCommand _mysqlCommand = new MySqlCommand();

        #region Constructor/Destructor & Dispose
        protected Command()
        {
        }
        ~Command()
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
                MySqlCommand command = null;
                command = Interlocked.Exchange(ref _mysqlCommand, command);
                if (null != command)
                    command.Dispose();
            }
        }
        #endregion Constructor/Destructor & Dispose

        public int Execute()
        {
            if (null != _mysqlCommand)
                return _mysqlCommand.ExecuteNonQuery();	// affected row count

            throw new System.ObjectDisposedException(
                "invalid attempt to call Execute() when Motors.MySql.Command is disposed.");
        }

        public object GetValue()
        {
            if (null != _mysqlCommand)
                return _mysqlCommand.ExecuteScalar();

            throw new System.ObjectDisposedException(
                "invalid attempt to call GetValue() when Motors.MySql.Command is disposed.");
        }

        public MySqlDataReader GetRows()
        {
            if (null != _mysqlCommand)
                return _mysqlCommand.ExecuteReader();

            // _mysqlCommand 가 null 인 경우에는 Query 또는 Procedure 객체가 Dispose 되었을 것이다.
            // 이는 Query/Procedure 객체를 사용하는 함수가 Fetch 류 함수의 리턴값(IEnumerable<T>)을 그대로 리턴하는 경우에 발생한다.
            //		*주의: IEnumerable 을 리턴하는 함수 코드는 IEnumerable 을 순회할 때, 비로소 함수 블럭이 실행된다.
            //			  결국, Query/Procedure 객체가 Dispose 된 이후에 Fetch 가 호출되게 되니 주의할 것.
            //			  팁) Query/Procedure 의 using 블럭 내에 yield 키워드가 있으면 괜찮음...
            throw new System.ObjectDisposedException(
                "invalid attempt to call Fetch() when Motors.MySql.Command is disposed.");
        }

        public void AddParam(string paramName, Enum value)
        {
            Debug.Assert(null != paramName, "paramName is null");

            if (false == string.IsNullOrEmpty(paramName))
            {
                var param = _mysqlCommand.Parameters.Add(paramName, MySqlDbType.Enum);
                param.Value = value;
                param.Direction = System.Data.ParameterDirection.Input;
            }
            else
            {
                throw new System.ArgumentException(
                    string.Format("parameter name can not be null. (value={0})", value.ToString()));
            }
        }
        public void AddParam(string paramName, object value)
        {
            Debug.Assert(null != paramName, "paramName is null");

            if (false == string.IsNullOrEmpty(paramName))
            {
                var param = _mysqlCommand.Parameters.AddWithValue(paramName, value ?? System.DBNull.Value);
                param.Direction = System.Data.ParameterDirection.Input;
            }
            else
            {
                throw new System.ArgumentException(
                    string.Format("parameter name can not be null. (value={0})", value.ToString()));
            }
        }

        // $주의: 'WHERE IN (@param)' 구문에는 동작하지 않는다...
        public void AddParam<T>(string paramName, IList<T> values, string separator = ",")
        {
            Debug.Assert(null != paramName, "paramName is null");

            if (false == string.IsNullOrEmpty(paramName))
            {
                string paramValue = null;
                if (null != values)
                {
                    var sb = new System.Text.StringBuilder();

                    bool is_first = true;
                    foreach (var value in values)
                    {
                        if (false == is_first) sb.Append(separator);
                        is_first = false;
                        sb.AppendFormat("{0}", value);
                    }

                    paramValue = sb.ToString();
                }

                var param = _mysqlCommand.Parameters.AddWithValue(paramName, paramValue ?? (object)System.DBNull.Value);
                param.Direction = System.Data.ParameterDirection.Input;
            }
            else
            {
                throw new System.ArgumentException(
                    string.Format("parameter name can not be null. (value type 'List<{0}>')", typeof(T).Name));
            }
        }

        // $주의: 'WHERE IN (@param)' 구문에는 동작하지 않는다...
        public void AddParam<T>(string paramName, T[] values, string separator = ",")
        {
            Debug.Assert(null != paramName, "paramName is null");

            if (false == string.IsNullOrEmpty(paramName))
            {
                string paramValue = null;
                if (null != values)
                {
                    var sb = new System.Text.StringBuilder();

                    bool is_first = true;
                    foreach (var value in values)
                    {
                        if (false == is_first) sb.Append(separator);
                        is_first = false;
                        sb.AppendFormat("{0}", value);
                    }

                    paramValue = sb.ToString();
                }

                var param = _mysqlCommand.Parameters.AddWithValue(paramName, paramValue ?? (object)System.DBNull.Value);
                param.Direction = System.Data.ParameterDirection.Input;
            }
            else
            {
                throw new System.ArgumentException(
                    string.Format("parameter name can not be null. (value type '{0}[]')", typeof(T).Name));
            }
        }

        public void AddParam(string paramName, byte[] value)
        {
            Debug.Assert(null != paramName, "paramName is null");

            if (false == string.IsNullOrEmpty(paramName))
            {
                var param = _mysqlCommand.Parameters.Add(paramName, MySqlDbType.Blob);

                string base64String = null;
                if (null != value) base64String = System.Convert.ToBase64String(value);

                param.Value = base64String ?? (object)System.DBNull.Value;
                param.Direction = System.Data.ParameterDirection.Input;
            }
            else
            {
                throw new System.ArgumentException(
                    string.Format("parameter name can not be null. (value type 'byte[]')"));
            }
        }
    }
}
