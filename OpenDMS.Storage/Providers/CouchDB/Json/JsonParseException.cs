using System;

namespace OpenDMS.Storage.Providers.CouchDB.Json
{
    public class JsonParseException : Exception
    {
        public JsonParseException()
            : base()
        {
        }

        public JsonParseException(string message)
            : base(message)
        {
        }

        public JsonParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
