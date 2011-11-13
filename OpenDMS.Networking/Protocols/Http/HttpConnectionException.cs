using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpConnectionException : Exception
    {
        public HttpConnectionException()
            : base()
        {
        }

        public HttpConnectionException(string message)
            : base(message)
        {
        }

        public HttpConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
