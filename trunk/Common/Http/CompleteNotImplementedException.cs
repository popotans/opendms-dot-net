using System;

namespace Common.Http
{
    public class CompleteNotImplementedException : Exception
    {
        public CompleteNotImplementedException(string message)
            : base(message)
        {
        }

        public CompleteNotImplementedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
