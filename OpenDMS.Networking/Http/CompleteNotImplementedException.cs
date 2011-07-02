using System;

namespace OpenDMS.Networking.Http
{
    public class CompleteNotImplementedException : Exception
    {
		#region Constructors (2) 

        public CompleteNotImplementedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CompleteNotImplementedException(string message)
            : base(message)
        {
        }

		#endregion Constructors 
    }
}
