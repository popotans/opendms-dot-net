using System;

namespace OpenDMS.Networking.Http
{
    public class ContentLengthExceededException : Exception
    {
		#region Constructors (2) 

        public ContentLengthExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ContentLengthExceededException(string message)
            : base(message)
        {
        }

		#endregion Constructors 
    }
}
