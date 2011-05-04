using System;

namespace Common.Http.Network
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
