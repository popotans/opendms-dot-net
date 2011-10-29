using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class MethodParseException : Exception
    {
        public MethodParseException()
            : base()
        {
        }

        public MethodParseException(string message)
            : base(message)
        {
        }

        public MethodParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
