using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paul.Utility
{
    static class Convert
    {
        public static bool ToBoolean(object value)
        {
            string str = value.ToString();
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
            return false;
        }

        public static T ToEnum<T>(object param) where T : struct
        {
            try
            {
                return (T)Enum.Parse(typeof(T), param.ToString());
            }
            catch (System.Exception e)
            {
                throw new System.ArgumentException(
                    string.Format("{0}\n   Convert.ToEnum<{1}>(param is '{2}')",
                        e.Message, typeof(T).FullName, (null == param) ? "null" : param),
                    e);
            }
        }

    }
}
