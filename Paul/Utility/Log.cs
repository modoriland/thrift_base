using System;
using System.Diagnostics;

sealed class Log
{
    private static log4net.ILog _log = null;

    public static void Initialize(string configFileName = null)
    {
        if (null == configFileName)
            configFileName = string.Format("{0}.config", System.Reflection.Assembly.GetEntryAssembly().Location);

        var fileInfo = new System.IO.FileInfo(configFileName);
        if (false == fileInfo.Exists)
        {
            throw new System.IO.FileNotFoundException(
                string.Format("could not found configuration (file={0})", configFileName));
        }

        log4net.Config.XmlConfigurator.Configure(fileInfo);
        _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    //////////////////////////////////////////////////////////////////////////

    public static bool IsFatalEnabled { get { return (null != _log) ? _log.IsFatalEnabled : false; } }

    public static void Fatal(object message)
    {
        if (true == Log.IsFatalEnabled)
            _log.Fatal(message);
    }
    public static void Fatal(string format, params object[] arg)
    {
        if (true == Log.IsFatalEnabled)
            _log.FatalFormat(format, arg);
    }

    //////////////////////////////////////////////////////////////////////////

    public static bool IsErrorEnabled { get { return (null != _log) ? _log.IsErrorEnabled : false; } }

    public static void Error(object message)
    {
        if (true == Log.IsErrorEnabled)
            _log.Error(message);
    }
    public static void Error(string format, params object[] arg)
    {
        if (true == Log.IsErrorEnabled)
            _log.ErrorFormat(format, arg);
    }

    //////////////////////////////////////////////////////////////////////////

    public static bool IsWarnEnabled { get { return (null != _log) ? _log.IsWarnEnabled : false; } }

    public static void Warn(object message)
    {
        if (true == Log.IsWarnEnabled)
            _log.Warn(message);
    }
    public static void Warn(string format, params object[] arg)
    {
        if (true == Log.IsWarnEnabled)
            _log.WarnFormat(format, arg);
    }

    //////////////////////////////////////////////////////////////////////////

    public static bool IsInfoEnabled { get { return (null != _log) ? _log.IsInfoEnabled : false; } }

    public static void Info(object message)
    {
        if (true == Log.IsInfoEnabled)
            _log.Info(message);
    }
    public static void Info(string format, params object[] arg)
    {
        if (true == Log.IsInfoEnabled)
            _log.InfoFormat(format, arg);
    }

    //////////////////////////////////////////////////////////////////////////

    public static bool IsDebugEnabled { get { return (null != _log) ? _log.IsDebugEnabled : false; } }

    public static void Debug(object message)
    {
        if (true == Log.IsDebugEnabled)
            _log.Debug(message);
    }
    public static void Debug(string format, params object[] arg)
    {
        if (true == Log.IsDebugEnabled)
            _log.DebugFormat(format, arg);
    }

    //////////////////////////////////////////////////////////////////////////

    public static string StackInfo(int skipFrame = 2)
    {
        var sb = new System.Text.StringBuilder();
        StackTrace st = new StackTrace(true);
        var frameCount = Math.Min(st.FrameCount, 10);
        bool is_first = true;
        for (int i = skipFrame; i < frameCount; i++)
        {
            StackFrame sf = st.GetFrame(i);
            sb.AppendFormat("{0}  at {1} [0x{2:X5}] in {3}:{4}", (true == is_first) ? "" : "\n",
                sf.GetMethod(), sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber());
            is_first = false;
        }
        return sb.ToString();
    }

    public static void Exception(System.Exception e)
    {
        if (true == Log.IsFatalEnabled)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0}: {1}", e.GetType(), e.Message);

            StackTrace st = new StackTrace(true);
            var frameCount = Math.Min(st.FrameCount, 10);
            for (int i = 1; i < frameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                sb.AppendFormat("\n  at {0} [0x{1:X5}] in {2}:{3}",
                    sf.GetMethod(), sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber());
            }

            Log.Fatal(sb.ToString());
        }
    }
    public static void Exception(System.Exception e, object message)
    {
        if (true == Log.IsFatalEnabled)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0}: {1}", e.GetType(), e.Message);

            StackTrace st = new StackTrace(true);
            var frameCount = Math.Min(st.FrameCount, 10);
            for (int i = 1; i < frameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                sb.AppendFormat("\n  at {0} [0x{1:X5}] in {2}:{3}",
                    sf.GetMethod(), sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber());
            }

            Log.Fatal("{0}\n{1}", message, sb.ToString());
        }
    }
    public static void Exception(System.Exception e, string format, params object[] arg)
    {
        if (true == Log.IsFatalEnabled)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0}: {1}", e.GetType(), e.Message);

            StackTrace st = new StackTrace(true);
            var frameCount = Math.Min(st.FrameCount, 10);
            for (int i = 1; i < frameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                sb.AppendFormat("\n  at {0} [0x{1:X5}] in {2}:{3}",
                    sf.GetMethod(), sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber());
            }

            Log.Fatal("{0}\n{1}", string.Format(format, arg), sb.ToString());
        }
    }

    //////////////////////////////////////////////////////////////////////////

    //delegate void LogAction(string format, params object[] args);
    //public static Action<string, object[]> GetDebugLogger(ProtoBuf.ErrCode errCode)
    //{
    //	//if (ProtoBuf.ErrCode.E_SUCCESS != errCode)
    //	//	return (format, arg) => { Log.Error(format, arg); };

    //	//return new delegate (string format, params object[] arg) { Log.Debug(format, arg); };
    //	return new Action<string, object[]>(format, args) { };
    //}
}
