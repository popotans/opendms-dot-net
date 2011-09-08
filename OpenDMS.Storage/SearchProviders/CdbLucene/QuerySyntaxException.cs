using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public class QuerySyntaxException : Exception
    {
        public QuerySyntaxException()
            : base()
        {
        }

        public QuerySyntaxException(string message)
            : base(message)
        {
        }

        public QuerySyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
