using System;

namespace OpenDMS.IO
{
    public class AccessException : Exception
    {
        public AccessException()
            : base()
        {
        }

        public AccessException(string message)
            : base(message)
        {
        }

        public AccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
