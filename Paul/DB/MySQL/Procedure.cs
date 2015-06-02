using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Paul.DB.MySQL
{
    public class Procedure : Command
    {
        // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
        // 그 전에는 null 이 리턴되기 때문에, ReaderState 를 두어서 GetOutParam() 을 호출할 때, read 상태인지 여부를 체크하도록 했다.
        #region Reader state check class
        private class ReaderState
        {
            public bool Complete { get; set; }
        }
        private struct ScopedReaderState : IDisposable
        {
            private Paul.DB.MySQL.Procedure.ReaderState _state;
            public ScopedReaderState(Paul.DB.MySQL.Procedure.ReaderState state) { _state = state; _state.Complete = false; }
            public void Dispose() { _state.Complete = true; }
        }

        private ReaderState _readerState = new ReaderState();
        #endregion Reader state check class

        public Procedure(Paul.DB.MySQL.Connection connection, string spName, Connection.Mode mode = Connection.Mode.RW)
        {
            if (connection.IsReadOnly && Connection.Mode.RW == mode)
            {
                Log.Fatal("cannot use Motors.MySql.Connection for writing: connection is read-only\n{0}", Log.StackInfo());
            }

            _mysqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            _mysqlCommand.CommandText = spName;
            _mysqlCommand.EnableCaching = false;
            _mysqlCommand.Connection = connection.GetConnection();
            // MySql 은 중첩된 Transaction 이 Stack 구조로 되어 있지 않다.
            // Adhoc query 와 Store Procedure 사이에서 각기 Transaction 을 사용할 때, 의도치 않은 결과를 초래 할 수 있으므로, 주의가 필요하다.
            //	예) sp 내부에서는 commit, 호출한 곳에서는 rollback. -> 이 경우 이미 commit 된 상태이므로, rollback 이 안된다.
            _mysqlCommand.Transaction = connection.GetTransaction();
            _readerState.Complete = true;
        }

        public T Execute<T>(Func<OutParam, T> fetcher)
        {
            Execute();

            if (null != _outputParamName)
                return fetcher(GetOutParam(_outputParamName));

            throw new System.Data.SqlTypes.SqlNotFilledException("not register output parameter");
        }

        private string _outputParamName = null;
        public void SetOutParam(string paramName, MySqlDbType type)
        {
            Debug.Assert(null != paramName, "paramerterName is null");

            if (null != paramName)
            {
                _outputParamName = paramName;
                var param = base._mysqlCommand.Parameters.Add(paramName, type);
                param.Direction = System.Data.ParameterDirection.Output;

            }
        }

        public OutParam GetOutParam(string outputParamName)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 가 완료 되었는지 여부를 체크한다. (record fetch 가 완료되지 않으면 에러로 처리..)
            if (true == _readerState.Complete)
            {
                var param = _mysqlCommand.Parameters[outputParamName];
                if (param == null || System.Convert.IsDBNull(param.Value))
                {
                    throw new System.Data.SqlTypes.SqlNullValueException(string.Format(
                        "Parameter '{0}' is null. This method or property cannot be called on null values.", outputParamName));
                }
                return new OutParam(_mysqlCommand.Parameters[outputParamName]);
            }

            throw new System.Data.SqlTypes.SqlNotFilledException(string.Format(
                "Parameter '{0}' is not yet available.\n   {1}\n   {2}",
                outputParamName,
                "If you use a stored procedure that returns information through output parameters or a return value,",
                "this information won't be available until after you close the DataReader because the stored procedure will still be executing."
                ));
        }
        public IEnumerable<T> FetchAll<T>(Func<ResultSet, T> fetcher)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                while (resultSet.Next())
                    yield return fetcher(resultSet);
            }
        }
        public IEnumerable<T> FetchAll<C, T>(Func<ResultSet, C, T> fetcher, C context)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                while (resultSet.Next())
                    yield return fetcher(resultSet, context);
            }
        }
        public T Fetch<T>(Func<ResultSet, T> fetcher)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                if (resultSet.Next())
                    return fetcher(resultSet);
                return default(T);
            }
        }
        public T Fetch<C, T>(Func<ResultSet, C, T> fetcher, C context)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                if (resultSet.Next())
                    return fetcher(resultSet, context);
                return default(T);
            }
        }
        public void FetchAll(Action<ResultSet> fetcher)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                while (resultSet.Next())
                    fetcher(resultSet);
            }
        }
        public void Fetch(Action<ResultSet> fetcher)
        {
            // MySqlConnector.Net 에서 Stored Procedure 의 Output 파라미터는 MySqlReader 가 Close 될때 채워진다.
            // 그 전에는 null 이 리턴되기 때문에, read 상태인지 여부를 확인 할 수 있도록 처리..
            using (var readerState = new ScopedReaderState(_readerState))
            using (var resultSet = new ResultSet(base.GetRows()))
            {
                if (resultSet.Next())
                    fetcher(resultSet);
            }
        }
    }
}
