using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class InvalidStageException : Exception
    {
        public InvalidStageException()
            : base()
        {
        }

        public InvalidStageException(string message)
            : base(message)
        {
        }

        public InvalidStageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
