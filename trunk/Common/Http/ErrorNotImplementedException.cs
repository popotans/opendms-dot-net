using System;

namespace Common.Http
{
    public class ErrorNotImplementedException : Exception
    {
        public ErrorNotImplementedException(string message)
            : base(message)
        {
        }

        public ErrorNotImplementedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
