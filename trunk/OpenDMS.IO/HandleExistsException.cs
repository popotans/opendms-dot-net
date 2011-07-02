using System;

namespace OpenDMS.IO
{
    public class HandleExistsException : Exception
    {
		#region Constructors (3) 

        public HandleExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HandleExistsException(string message)
            : base(message)
        {
        }

        public HandleExistsException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
