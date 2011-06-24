using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class ActiveTransactionException : Exception
    {
        public ActiveTransactionException()
            : base()
        {
        }

        public ActiveTransactionException(string message)
            : base(message)
        {
        }

        public ActiveTransactionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
