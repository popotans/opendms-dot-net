using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class ContentLengthExceededException : Exception
    {
		#region Constructors (3) 

        public ContentLengthExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ContentLengthExceededException(string message)
            : base(message)
        {
        }

        public ContentLengthExceededException()
            : base()
        {
        }

		#endregion Constructors 
    }
}
