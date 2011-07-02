using System;

namespace OpenDMS.Networking.Http
{
    public class ErrorNotImplementedException : Exception
    {
		#region Constructors (2) 

        public ErrorNotImplementedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ErrorNotImplementedException(string message)
            : base(message)
        {
        }

		#endregion Constructors 
    }
}
