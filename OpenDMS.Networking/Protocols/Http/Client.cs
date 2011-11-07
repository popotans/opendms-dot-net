using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Client
    {
        private ulong _bytesReceivedContentOnly = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesSentHeadersOnly = 0;
        private ulong _bytesSentTotal = 0;

        private ulong _contentLengthRx = 0;
        private ulong _contentLengthTx = 0;
        private ulong _headersLengthRx = 0;
        private ulong _headersLengthTx = 0;

        private bool _isBusy = false;

        private int _receiveBufferSize = 8192;
        private int _receiveTimeout = 60000;
        private int _sendBufferSize = 8192;
        private int _sendTimeout = 60000;
        
        public HttpConnection
    }
}
