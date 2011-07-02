using System;

namespace OpenDMS.Networking.Http
{
    public class UnsupportedException : Exception
    {
		#region Constructors (3) 

        public UnsupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnsupportedException(string message)
            : base(message)
        {
        }

        public UnsupportedException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
