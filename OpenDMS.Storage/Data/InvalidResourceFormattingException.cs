using System;

namespace OpenDMS.Storage.Data
{
    class InvalidResourceFormattingException : Exception
    {
        public InvalidResourceFormattingException()
            : base()
        {
        }

        public InvalidResourceFormattingException(string message)
            : base(message)
        {
        }

        public InvalidResourceFormattingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
