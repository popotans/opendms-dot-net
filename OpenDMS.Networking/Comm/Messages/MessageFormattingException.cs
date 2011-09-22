using System;

namespace OpenDMS.Networking.Comm.Messages
{
    public class MessageFormattingException : Exception
    {
        public MessageFormattingException()
            : base()
        {
        }

        public MessageFormattingException(string message)
            : base(message)
        {
        }

        public MessageFormattingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
