using System;

namespace OpenDMS.Networking.Http
{
    public class HttpNetworkException : Exception
    {
		#region Constructors (2) 

        public HttpNetworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HttpNetworkException(string message)
            : base(message)
        {
        }

		#endregion Constructors 
    }
}
