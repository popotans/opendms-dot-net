using System;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class RevisionException : Exception
    {
        public RevisionException()
            : base()
        {
        }

        public RevisionException(string message)
            : base(message)
        {
        }

        public RevisionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
