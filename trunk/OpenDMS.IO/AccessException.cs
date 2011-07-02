using System;

namespace OpenDMS.IO
{
    public class AccessException : Exception
    {
		#region Constructors (3) 

        public AccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AccessException(string message)
            : base(message)
        {
        }

        public AccessException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
