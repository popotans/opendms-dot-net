using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpNetworkStreamException : Exception
    {
        public HttpNetworkStreamException()
            : base()
        {
        }

        public HttpNetworkStreamException(string message)
            : base(message)
        {
        }

        public HttpNetworkStreamException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
