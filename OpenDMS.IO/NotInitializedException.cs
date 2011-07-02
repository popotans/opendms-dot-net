using System;

namespace OpenDMS.IO
{
    public class NotInitializedException : Exception
    {
		#region Constructors (3) 

        public NotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotInitializedException(string message)
            : base(message)
        {
        }

        public NotInitializedException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
