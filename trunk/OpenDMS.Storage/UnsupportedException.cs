using System;

namespace OpenDMS.Storage
{
    public class UnsupportedException : Exception
    {
        public UnsupportedException(string message)
            : base(message)
        {
        }

        public UnsupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
