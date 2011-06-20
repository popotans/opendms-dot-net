using System;

namespace OpenDMS.IO
{
    public class HandleExistsException : Exception
    {
        public HandleExistsException()
            : base()
        {
        }

        public HandleExistsException(string message)
            : base(message)
        {
        }

        public HandleExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
