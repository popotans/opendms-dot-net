using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpConnection
    {
        private ulong _bytesReceivedContentOnly = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesSentHeadersOnly = 0;
        private ulong _bytesSentTotal = 0;

        private Tcp.TcpConnection _tcpConnection;

        public HttpConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
        }
    }
}
