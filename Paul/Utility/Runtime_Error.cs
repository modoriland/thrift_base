using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Paul
{
    [Serializable]
    [ComVisible(true)]
    public class RuntimeException : System.ApplicationException
    {
        private System.Exception _exception = null;

        public RuntimeException()
            : base()
        {
        }

        public RuntimeException(string message)
            : base(message)
        {
        }

        public RuntimeException(string message, Exception exception)
            : base(message, exception)
        {
            _exception = exception;
        }

        protected RuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string StackTrace
        {
            get
            {
                if (null != _exception)
                    return _exception.StackTrace;
                else
                    return base.StackTrace;
            }
        }

    }
}
