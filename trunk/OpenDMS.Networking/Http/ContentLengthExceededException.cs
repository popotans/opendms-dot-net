using System;

namespace OpenDMS.Networking.Http
{
    public class ContentLengthExceededException : Exception
    {
        public ContentLengthExceededException(string message)
            : base(message)
        {
        }

        public ContentLengthExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
