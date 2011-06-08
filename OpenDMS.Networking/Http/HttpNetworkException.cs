using System;

namespace OpenDMS.Networking.Http
{
    public class HttpNetworkException : Exception
    {
        public HttpNetworkException(string message)
            : base(message)
        {
        }

        public HttpNetworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
