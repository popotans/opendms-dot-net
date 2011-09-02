using System;

namespace OpenDMS.Storage.Data
{
    public class InvalidVersionFormattingException : Exception
    {
        public InvalidVersionFormattingException()
            : base()
        {
        }

        public InvalidVersionFormattingException(string message)
            : base(message)
        {
        }

        public InvalidVersionFormattingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
