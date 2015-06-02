using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Paul.DB.MySQL
{
    static class ResultSet_Extension
    {
        public static string GetString(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToString(value);
            return null;
        }
        public static bool? GetBool(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var str = resultSet.GetString(column);
            if (null == str)
                return null;

            {
                bool result = false;
                if (true == bool.TryParse(str, out result))
                    return result;
            }

            {
                int result = 0;
                if (true == int.TryParse(str, out result))
                    return result != 0 ? true : false;
            }
            return null;
        }
        public static byte? GetByte(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToByte(value);
            return null;
        }
        public static char? GetChar(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToChar(value);
            return null;
        }
        public static short? GetInt16(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToInt16(value);
            return null;
        }
        public static ushort? GetUInt16(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToUInt16(value);
            return null;
        }
        public static int? GetInt32(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToInt32(value);
            return null;
        }
        public static uint? GetUInt32(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToUInt32(value);
            return null;
        }
        public static long? GetInt64(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToInt64(value);
            return null;
        }
        public static ulong? GetUInt64(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToUInt64(value);
            return null;
        }
        public static float? GetFloat(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return (float)System.Convert.ToDouble(value);
            return null;
        }
        public static byte[] GetByteArray(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
            {
                return System.Convert.FromBase64String(
                    System.Text.Encoding.ASCII.GetString((byte[])value));
            }
            return null;
        }
        public static IEnumerable<object> GetSplitValue(this Paul.DB.MySQL.ResultSet resultSet, string column, char delim = ',')
        {
            var value = resultSet.GetValue(column);
            if (null != value)
            {
                var str = value.ToString();

                char[] delims = { delim };
                string[] array = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                foreach (var elem in array)
                    yield return elem.Trim();
            }
        }

        public static IEnumerable<T> GetSplitValue<T>(this Paul.DB.MySQL.ResultSet resultSet, string column, char delim, Func<object, T> convert)
        {
            foreach (var elem in resultSet.GetSplitValue(column, delim))
                yield return convert(elem);
        }

        public static Nullable<T> GetEnum<T>(this Paul.DB.MySQL.ResultSet resultSet, string column) where T : struct
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return Paul.Utility.Convert.ToEnum<T>(value);
            return null;
        }

        // $TODO: not yet test...
        public static DateTime? GetDateTime(this Paul.DB.MySQL.ResultSet resultSet, string column)
        {
            var value = resultSet.GetValue(column);
            if (null != value)
                return System.Convert.ToDateTime(value);	// DateTime 의 DateTimeKind 가 Utc 로 생성된다.
            return null;
        }

        // epoch unix_timestamp - epoch DateTime
        private static long EPOCH_DATETIME = (long)(new DateTime(1970, 1, 1, 0, 0, 0) - new DateTime(1, 1, 1, 0, 0, 0)).TotalSeconds;

        // $TODO: not yet test...
        //public static DateTime? GetDateTimeFromUnixTimestamp(this Paul.DB.MySQL.ResultSet resultSet, string column, DateTimeKind kind = DateTimeKind.Local)
        //{
        //	var unix_timestamp = GetValue(column);
        //	if (null != unix_timestamp)
        //	{
        //		var dateTimeTicks = (long)(Column.ToInt64(unix_timestamp) + EPOCH_DATETIME) * TimeSpan.TicksPerSecond;
        //		var dateTime = new DateTime(dateTimeTicks, DateTimeKind.Utc);

        //		if (DateTimeKind.Utc == kind)
        //			return dateTime;
        //		else
        //			return dateTime.ToLocalTime();
        //	}
        //	return null;
        //}

        //public static long GetUtcTicksFromUnixTimestamp(this Paul.DB.MySQL.ResultSet resultSet, string column, long defaultValue)
        //{
        //	var unix_timestamp = GetValue(column);
        //	if (null != unix_timestamp)
        //		return (Column.ToInt64(unix_timestamp) + EPOCH_DATETIME) * TimeSpan.TicksPerSecond;
        //	return defaultValue;
        //}

        //public static long? GetUtcTicksFromUnixTimestamp(this Paul.DB.MySQL.ResultSet resultSet, string column)
        //{
        //	var unix_timestamp = GetValue(column);
        //	if (null != unix_timestamp)
        //		return (Column.ToInt64(unix_timestamp) + EPOCH_DATETIME) * TimeSpan.TicksPerSecond;
        //	return null;
        //}
    }

    static class OutParam_Extension
    {
        public static bool ToBool(this Paul.DB.MySQL.OutParam param)
        {
            var str = param.ToString();
            bool boolValue = false;
            if (true == bool.TryParse(str, out boolValue))
                return boolValue;
            else
            {
                int intValue = 0;
                if (true == int.TryParse(str, out intValue))
                    return intValue != 0 ? true : false;
            }
            return false;
        }
        public static byte ToByte(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToByte(param.Value);
        }
        public static char ToChar(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToChar(param.Value);
        }
        public static short ToInt16(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToInt16(param.Value);
        }
        public static ushort ToUInt16(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToUInt16(param.Value);
        }
        public static int ToInt32(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToInt32(param.Value);
        }
        public static uint ToUInt32(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToUInt32(param.Value);
        }
        public static long ToInt64(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToInt64(param.Value);
        }
        public static ulong ToUInt64(this Paul.DB.MySQL.OutParam param)
        {
            return System.Convert.ToUInt64(param.Value);
        }
        public static float ToFloat(this Paul.DB.MySQL.OutParam param)
        {
            return (float)System.Convert.ToDouble(param.Value);
        }
        public static T ToEnum<T>(this Paul.DB.MySQL.OutParam param) where T : struct
        {
            return Paul.Utility.Convert.ToEnum<T>(param.Value);
        }
        public static IEnumerable<object> ToSplitValue(this Paul.DB.MySQL.OutParam param, char delim = ',')
        {
            var str = param.ToString();
            if (null != str)
            {
                char[] delims = { delim };
                string[] array = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                foreach (var elem in array)
                    yield return elem.Trim();
            }
        }
        public static IEnumerable<T> ToSplitValue<T>(this Paul.DB.MySQL.OutParam param, char delim, Func<object, T> convert)
        {
            foreach (var elem in param.ToSplitValue(delim))
                yield return convert(elem);
        }
    }
}
