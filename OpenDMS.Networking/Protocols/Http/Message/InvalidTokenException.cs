using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class InvalidTokenException : Exception
    {
		#region Constructors (3) 

        public InvalidTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidTokenException(string message)
            : base(message)
        {
        }

        public InvalidTokenException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
