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
        public delegate void ProgressDelegate(HttpConnection sender, Tcp.DirectionType direction, int packetSize, decimal requestPercentSent, decimal responsePercentReceived);
        public delegate void CompletionDelegate(HttpConnection sender, Response response);
        
        private event ResolverDelegate OnHostResolved;
        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ErrorDelegate OnError;
        public event ConnectionDelegate OnTimeout;
        public event ProgressDelegate OnProgress;
        public event CompletionDelegate OnComplete;
        
        private ulong _bytesReceivedContentOnly = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesSentHeadersOnly = 0;
        private ulong _bytesSentTotal = 0;

        private Tcp.TcpConnection _tcpConnection;
        private IPHostEntry _remoteHostEntry;
        private ResponseBuilder _responseBuilder;
        private Tcp.Params.Buffer _receiveBufferSettings;
        private Tcp.Params.Buffer _sendBufferSettings;

        public Uri Uri { get; private set; }
        public bool IsConnected { get { return _tcpConnection.IsConnected; } }

        public HttpConnection(Uri uri, Tcp.Params.Buffer receiveBufferSettings, 
            Tcp.Params.Buffer sendBufferSettings)
        {
            Uri = uri;
            _receiveBufferSettings = receiveBufferSettings;
            _sendBufferSettings = sendBufferSettings;
        }

        public HttpConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Uri = uri;
            _receiveBufferSettings = new Tcp.Params.Buffer() { Size = receiveBufferSize, Timeout = receiveTimeout };
            _sendBufferSettings = new Tcp.Params.Buffer() { Size = sendBufferSize, Timeout = sendTimeout };
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
            Tcp.Params.Connection param;
                        
            param = new Tcp.Params.Connection() 
            { 
                EndPoint = new IPEndPoint(_remoteHostEntry.AddressList[0], Uri.Port), 
                ReceiveBuffer = _receiveBufferSettings,
                SendBuffer = _sendBufferSettings
            };

            _tcpConnection = new Tcp.TcpConnection(param);

            _tcpConnection.OnConnect += new Tcp.TcpConnection.ConnectionDelegate(ConnectAsync_OnHostResolved_OnConnect);
            _tcpConnection.OnDisconnect += new Tcp.TcpConnection.ConnectionDelegate(ConnectAsync_OnHostResolved_OnDisconnect);
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(ConnectAsync_OnHostResolved_OnError);
            _tcpConnection.OnTimeout += new Tcp.TcpConnection.ConnectionDelegate(ConnectAsync_OnHostResolved_OnTimeout);

            _tcpConnection.ConnectAsync();
        }

        private void ConnectAsync_OnHostResolved_OnConnect(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            if (OnConnect != null) OnConnect(this);
        }

        private void ConnectAsync_OnHostResolved_OnDisconnect(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            if (OnDisconnect != null) OnDisconnect(this);
        }

        private void ConnectAsync_OnHostResolved_OnError(Tcp.TcpConnection sender2, string message, Exception exception)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            if (OnError != null) OnError(this, message, exception);
        }

        private void ConnectAsync_OnHostResolved_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            if (OnTimeout != null) OnTimeout(this);
        }

        public void DisconnectAsync()
        {
            _tcpConnection.OnDisconnect += new Tcp.TcpConnection.ConnectionDelegate(DisconnectAsync_OnDisconnect);
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(DisconnectAsync_OnError);

            _tcpConnection.DisconnectAsync();
        }

        public void DisconnectAsync_OnDisconnect(Tcp.TcpConnection sender2)
        {
            _tcpConnection.OnDisconnect -= DisconnectAsync_OnDisconnect;
            _tcpConnection.OnError -= DisconnectAsync_OnError;

            if (OnDisconnect != null) OnDisconnect(this);
        }

        public void DisconnectAsync_OnError(Tcp.TcpConnection sender2, string message, Exception exception)
        {
            _tcpConnection.OnDisconnect -= DisconnectAsync_OnDisconnect;
            _tcpConnection.OnError -= DisconnectAsync_OnError;

            if (OnError != null) OnError(this, message, exception);
        }

        public void SendRequestAsync(Request request)
        {
            if (!IsConnected)
                throw new HttpConnectionException("A network connection is not established.");

            Tcp.TcpConnection.ProgressDelegate onProgress;
            Tcp.TcpConnection.AsyncCallback callback;
            Stream stream;
            long requestSize = 0;
            long bytesSent = 0;

            // Make the RequestLine and Headers into a stream
            stream = request.MakeRequestLineAndHeadersStream();
            requestSize += stream.Length;

            if (request.Body.SendStream != null)
                requestSize += request.Body.SendStream.Length;
            
            onProgress = delegate(Tcp.TcpConnection sender, Tcp.DirectionType direction, int packetSize)
            {
                bytesSent += packetSize;
                if (OnProgress != null) OnProgress(this, direction, packetSize, ((decimal)bytesSent / (decimal)requestSize), 0);
            };

            _tcpConnection.OnProgress += onProgress;
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(SendRequest_OnError);
            _tcpConnection.OnTimeout += new Tcp.TcpConnection.ConnectionDelegate(SendRequest_OnTimeout);


            callback = delegate(Tcp.TcpConnection sender, Tcp.TcpConnectionAsyncEventArgs e)
            {
                // Do not yet disconnect the error and timeout event handlers as they will continue to function for the
                // Check100Continue
                //_tcpConnection.OnError -= onError;
                //_tcpConnection.OnTimeout -= onTimeout;

                Tcp.TcpConnection.AsyncCallback c100Callback, contentSendCallback;

                contentSendCallback = delegate(Tcp.TcpConnection sender3, Tcp.TcpConnectionAsyncEventArgs e3)
                { // Called when all the content is sent, need to receive
                    // Now we can unhook onError, onTimeout and onProgress because ReceiveResponseAsync will rehook
                    // for when it is called individually, so we don't want to risk double hooking

                    _tcpConnection.OnError -= SendRequest_OnError;
                    _tcpConnection.OnTimeout -= SendRequest_OnTimeout;
                    _tcpConnection.OnProgress -= onProgress;

                    ReceiveResponseAsync(request);
                };

                c100Callback = delegate(Tcp.TcpConnection sender2, Tcp.TcpConnectionAsyncEventArgs e2)
                {
                    _responseBuilder = new ResponseBuilder(request);

                    _responseBuilder.AppendAndParse(e2.Buffer, 0, e2.Length);

                    if (_responseBuilder.Response == null)
                        throw new HttpNetworkStreamException("Status 100 Continue not received.");

                    if (request.Body.ReceiveStream != null)
                        _tcpConnection.SendAsync(request.Body.ReceiveStream, contentSendCallback);
                };

                if (request.Headers.ContainsKey(new Message.Expect100ContinueHeader()))
                    Check100Continue(c100Callback);
                else
                {
                    if (request.Body.ReceiveStream != null)
                        _tcpConnection.SendAsync(request.Body.ReceiveStream, contentSendCallback);
                }
            };

            _tcpConnection.SendAsync(stream, callback);
        }

        private void SendRequest_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnError -= SendRequest_OnError;
            _tcpConnection.OnTimeout -= SendRequest_OnTimeout;

            if (OnTimeout != null) OnTimeout(this);
        }

        private void SendRequest_OnError(Tcp.TcpConnection sender, string message, Exception exception)
        {
            _tcpConnection.OnError -= SendRequest_OnError;
            _tcpConnection.OnTimeout -= SendRequest_OnTimeout;

            if (OnError != null) OnError(this, message, exception);
        }
        
        private void Check100Continue(Tcp.TcpConnection.AsyncCallback callback)
        {
            if (!IsConnected)
                throw new HttpConnectionException("A network connection is not established.");

            int loop = 0;

            // Loops every 0.25 seconds until either data is available or the receive timeout elapses
            while (_tcpConnection.BytesAvailable <= 0)
            {
                if ((_receiveBufferSettings.Timeout / 250) <= loop)
                    throw new TimeoutException("Timeout waiting for 100 continue status.");

                System.Threading.Thread.Sleep(250);
            }

            _tcpConnection.ReceiveAsync(callback);
        }
        
        public void ReceiveResponseAsync(Http.Request request)
        {
            if (!IsConnected)
                throw new HttpConnectionException("A network connection is not established.");

            long bytesReceived = 0;
            Tcp.TcpConnection.ProgressDelegate onProgress;
            ResponseBuilder.AsyncCallback callback;

            if (_responseBuilder == null)
                _responseBuilder = new ResponseBuilder(request);


            onProgress = delegate(Tcp.TcpConnection sender, Tcp.DirectionType direction, int packetSize)
            {
                bytesReceived += packetSize;
                if (OnProgress != null) OnProgress(this, direction, packetSize, 100, ((decimal)bytesReceived / (decimal)_responseBuilder.MessageSize));
            };

            _tcpConnection.OnProgress += onProgress;
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(ReceiveResponseAsync_OnError);
            _tcpConnection.OnTimeout += new Tcp.TcpConnection.ConnectionDelegate(ReceiveResponseAsync_OnTimeout);

            callback = delegate(MessageBuilder sender, Message.Base message)
            {
                if (OnComplete != null) OnComplete(this, (Response)message);
            };

            _responseBuilder.ParseAndAttachToBody(_tcpConnection, callback);
        }

        private void ReceiveResponseAsync_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnError -= ReceiveResponseAsync_OnError;
            _tcpConnection.OnTimeout -= ReceiveResponseAsync_OnTimeout;

            if (OnTimeout != null) OnTimeout(this);
        }

        private void ReceiveResponseAsync_OnError(Tcp.TcpConnection sender, string message, Exception exception)
        {
            _tcpConnection.OnError -= ReceiveResponseAsync_OnError;
            _tcpConnection.OnTimeout -= ReceiveResponseAsync_OnTimeout;

            if (OnError != null) OnError(this, message, exception);
        }
    }
}
