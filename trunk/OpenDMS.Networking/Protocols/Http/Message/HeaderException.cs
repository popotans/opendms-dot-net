using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class HeaderException : Exception
    {
        public HeaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HeaderException(string message)
            : base(message)
        {
        }

        public HeaderException()
            : base()
        {
        }
    }
}
