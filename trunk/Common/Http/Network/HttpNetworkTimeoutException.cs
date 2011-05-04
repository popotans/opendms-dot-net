using System;

namespace Common.Http.Network
{
    public class HttpNetworkTimeoutException : Exception
    {
        public HttpNetworkTimeoutException(string message)
            : base(message)
        {
        }

        public HttpNetworkTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
