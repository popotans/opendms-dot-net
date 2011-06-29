using System;

namespace OpenDMS.Storage.Security
{
    public class UnknownDatabaseException : Exception
    {
        public UnknownDatabaseException()
            : base()
        {
        }

        public UnknownDatabaseException(string message)
            : base(message)
        {
        }

        public UnknownDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
