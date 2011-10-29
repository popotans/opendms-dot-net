using System;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class TcpConnectionException : Exception
    {
        public TcpConnectionException()
            : base()
        {
        }

        public TcpConnectionException(string message)
            : base(message)
        {
        }

        public TcpConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
