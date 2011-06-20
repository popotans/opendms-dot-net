using System;

namespace OpenDMS.IO
{
    public class NotInitializedException : Exception
    {
        public NotInitializedException()
            : base()
        {
        }

        public NotInitializedException(string message)
            : base(message)
        {
        }

        public NotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
