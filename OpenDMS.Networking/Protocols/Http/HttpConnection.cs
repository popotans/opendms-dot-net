using System;
using System.IO;
using System.Net;

namespace OpenDMS.Networking.Protocols.Http
{
    public class HttpConnection
    {
        private delegate void ResolverDelegate(HttpConnection sender, IPHostEntry host);
        public delegate void ConnectionDelegate(HttpConnection sender);
        public delegate void ErrorDelegate(HttpConnection sender, string message, Exception exception);
        public delegate void ProgressDelegate(HttpConnection sender, Tcp.DirectionType direction, int packetSize);
        
        private event ResolverDelegate OnHostResolved;
        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ErrorDelegate OnError;
        public event ConnectionDelegate OnTimeout;
        public event ProgressDelegate OnProgress;

        private int _sendTimeout;
        private int _sendBufferSize;
        private int _receiveTimeout;
        private int _receiveBufferSize;

        private ulong _bytesReceivedContentOnly = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesSentHeadersOnly = 0;
        private ulong _bytesSentTotal = 0;

        private Tcp.TcpConnection _tcpConnection;
        private IPHostEntry _remoteHostEntry;

        public Uri Uri { get; private set; }
        public bool IsConnected { get { return _tcpConnection.IsConnected; } }

        public HttpConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Uri = uri;
            _sendTimeout = sendTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveTimeout = receiveTimeout;
            _receiveBufferSize = receiveBufferSize;
        }

        private void ResolveHostAsync()
        {
            Dns.BeginGetHostEntry(Uri.Host, ResolveHostAsync_Callback, null);
        }

        private void ResolveHostAsync_Callback(IAsyncResult ar)
        {
            _remoteHostEntry = Dns.EndGetHostEntry(ar);
            if (OnHostResolved != null) OnHostResolved(this, _remoteHostEntry);
        }

        public void ConnectAsync()
        {
            // Resolve Host
            OnHostResolved += new ResolverDelegate(ConnectAsync_OnHostResolved);
            ResolveHostAsync();
        }

        private void ConnectAsync_OnHostResolved(HttpConnection sender, IPHostEntry host)
        {
            Tcp.TcpConnection.ConnectionDelegate onConnect, onDisconnect, onTimeout;
            Tcp.TcpConnection.ErrorDelegate onError;

            Tcp.Params.Connection param;
                        
            param = new Tcp.Params.Connection() 
            { 
                EndPoint = new IPEndPoint(_remoteHostEntry.AddressList[0], Uri.Port), 
                ReceiveBuffer = new Tcp.Params.Buffer() { Size = _receiveBufferSize, Timeout = _receiveTimeout },
                SendBuffer = new Tcp.Params.Buffer() { Size = _sendBufferSize, Timeout = _sendTimeout }
            };

            _tcpConnection = new Tcp.TcpConnection(param);

            _tcpConnection.OnConnect += onConnect = delegate(Tcp.TcpConnection sender2)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnConnect != null) OnConnect(this);
            };
            _tcpConnection.OnDisconnect += onDisconnect = delegate(Tcp.TcpConnection sender2)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnDisconnect != null) OnDisconnect(this);
            };
            _tcpConnection.OnError += onError = delegate(Tcp.TcpConnection sender2, string message, Exception exception)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnError != null) OnError(this, message, exception);
            };
            _tcpConnection.OnTimeout += onTimeout = delegate(Tcp.TcpConnection sender2)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnTimeout != null) OnTimeout(this);
            };

            _tcpConnection.ConnectAsync();
        }

        public void DisconnectAsync()
        {
            Tcp.TcpConnection.ConnectionDelegate onDisconnect;
            Tcp.TcpConnection.ErrorDelegate onError;

            _tcpConnection.OnDisconnect += onDisconnect = delegate(Tcp.TcpConnection sender2)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnDisconnect != null) OnDisconnect(this);
            };
            _tcpConnection.OnError += onError = delegate(Tcp.TcpConnection sender2, string message, Exception exception)
            {
                _tcpConnection.OnDisconnect -= onDisconnect;
                _tcpConnection.OnConnect -= onConnect;
                _tcpConnection.OnError -= onError;
                _tcpConnection.OnTimeout -= onTimeout;

                if (OnError != null) OnError(this, message, exception);
            };

            _tcpConnection.DisconnectAsync();
        }

        public void SendRequest(Request request)
        {
            // Make sure we are connected
            // Send the Request Line and Headers
            // If there is an Expect 100-Continue header we need to wait for the 100-Continue status response
            // Send the Body

            if (!IsConnected)
                throw new HttpConnectionException("A network connection is not established.");

            Stream stream;


            stream = request.MakeRequestLineAndHeadersStream();

            _tcpConnection.SendAsync(

        }
    }
}
