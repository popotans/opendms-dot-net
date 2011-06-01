using System;

namespace Common.Data.Providers.CouchDB
{
    public class UnsupportedException : Exception
    {        
        public UnsupportedException(string message)
            : base(message)
        {
        }

        public UnsupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
