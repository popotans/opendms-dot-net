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
        public bool IsConnected { get { if (_tcpConnection == null) return false; else return _tcpConnection.IsConnected; } }

        public HttpConnection(Uri uri, Tcp.Params.Buffer receiveBufferSettings, 
            Tcp.Params.Buffer sendBufferSettings)
        {
            Uri = uri;
            _receiveBufferSettings = receiveBufferSettings;
            _sendBufferSettings = sendBufferSettings;
            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nHttpConnection created.");
        }

        public HttpConnection(Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
        {
            Uri = uri;
            _receiveBufferSettings = new Tcp.Params.Buffer() { Size = receiveBufferSize, Timeout = receiveTimeout };
            _sendBufferSettings = new Tcp.Params.Buffer() { Size = sendBufferSize, Timeout = sendTimeout };
            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nHttpConnection created.");
        }

        private void ResolveHostAsync()
        {
            IPAddress ip;

            if (IPAddress.TryParse(Uri.DnsSafeHost, out ip))
            {
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nUri received contained the host IP, thus no host resolution was required, continuing with the IP received.");

                _remoteHostEntry = new IPHostEntry();
                _remoteHostEntry.AddressList = new IPAddress[1];
                _remoteHostEntry.AddressList[0] = ip;
                if (OnHostResolved != null) OnHostResolved(this, _remoteHostEntry);
            }
            else
            {
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nAttempting to resolve host " + Uri.DnsSafeHost);

                Dns.BeginGetHostEntry(Uri.DnsSafeHost, ResolveHostAsync_Callback, null);
            }
        }

        private void ResolveHostAsync_Callback(IAsyncResult ar)
        {
            _remoteHostEntry = Dns.EndGetHostEntry(ar);

            if (_remoteHostEntry.AddressList.Length > 0)
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nRemote host resolved to " + _remoteHostEntry.AddressList[0].ToString());
            else
                Logger.Network.Warn("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nResolved remote host without any results.");

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

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection establishing connection to remote host using TcpConnection.");

            _tcpConnection.ConnectAsync();
        }

        private void ConnectAsync_OnHostResolved_OnConnect(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection connected to remote host.");

            if (OnConnect != null) OnConnect(this);
        }

        private void ConnectAsync_OnHostResolved_OnDisconnect(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection disconnected from remote host.");

            if (OnDisconnect != null) OnDisconnect(this);
        }

        private void ConnectAsync_OnHostResolved_OnError(Tcp.TcpConnection sender2, string message, Exception exception)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            Logger.Network.Error("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection encountered an error.\r\nMessage: " + message, exception);

            if (OnError != null) OnError(this, message, exception);
        }

        private void ConnectAsync_OnHostResolved_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnDisconnect -= ConnectAsync_OnHostResolved_OnDisconnect;
            _tcpConnection.OnConnect -= ConnectAsync_OnHostResolved_OnConnect;
            _tcpConnection.OnError -= ConnectAsync_OnHostResolved_OnError;
            _tcpConnection.OnTimeout -= ConnectAsync_OnHostResolved_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection connected to remote host.");

            if (OnTimeout != null) OnTimeout(this);
        }

        public void DisconnectAsync()
        {
            _tcpConnection.OnDisconnect += new Tcp.TcpConnection.ConnectionDelegate(DisconnectAsync_OnDisconnect);
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(DisconnectAsync_OnError);

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection disconnecting from remote host.");

            _tcpConnection.DisconnectAsync();
        }

        private void DisconnectAsync_OnDisconnect(Tcp.TcpConnection sender2)
        {
            _tcpConnection.OnDisconnect -= DisconnectAsync_OnDisconnect;
            _tcpConnection.OnError -= DisconnectAsync_OnError;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection disconnected from remote host.");

            if (OnDisconnect != null) OnDisconnect(this);
        }

        private void DisconnectAsync_OnError(Tcp.TcpConnection sender2, string message, Exception exception)
        {
            _tcpConnection.OnDisconnect -= DisconnectAsync_OnDisconnect;
            _tcpConnection.OnError -= DisconnectAsync_OnError;

            Logger.Network.Error("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection encountered an error while disconnecting from remote host\r\nMessage: " + message, exception);

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
            if (request.Body.SendStream != null)
                stream = request.MakeRequestLineAndHeadersStream();
            else
                stream = request.MakeRequestLineAndHeadersStream("\r\n");

            requestSize += stream.Length;

            if (stream != null)
                requestSize += stream.Length;
            
            onProgress = delegate(Tcp.TcpConnection sender, Tcp.DirectionType direction, int packetSize)
            {
                bytesSent += packetSize;
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + 
                    "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() +
                    "\r\nBytes Sent: " + bytesSent.ToString() + 
                    "\r\nRequest Size: " + requestSize.ToString() + 
                    "\r\nPacket Size: " + packetSize.ToString() +
                    "\r\nHttpConnection reporting progress sending to remote host.");
                if (OnProgress != null) OnProgress(this, direction, packetSize, ((decimal)((decimal)bytesSent / (decimal)requestSize)), 0);
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

                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection sent the stream.");

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

                    _responseBuilder.AppendAndParse(e2.Buffer, 0, e2.BytesTransferred);

                    if (_responseBuilder.Response == null)
                    {
                        Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection did not receive the expected 100-Continue response.");
                        throw new HttpNetworkStreamException("Status 100 Continue not received.");
                    }

                    Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection received the expected 100-Continue response.");

                    if (request.Body.SendStream != null)
                    {
                        Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection requesting TcpConnection to send the body content.");
                        _tcpConnection.SendAsync(request.Body.SendStream, contentSendCallback);
                    }
                };

                if (request.Headers.ContainsKey(new Message.Expect100ContinueHeader()))
                {
                    Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nThe request was sent with a Expect: 100-Continue header, now checking for the 100-Continue response.");

                    Check100Continue(c100Callback);
                }
                else
                {
                    if (request.Body.SendStream != null)
                    {
                        Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection requesting TcpConnection to send the body content.");
                        _tcpConnection.SendAsync(request.Body.SendStream, contentSendCallback);
                    }
                    else
                    {
                        _tcpConnection.OnError -= SendRequest_OnError;
                        _tcpConnection.OnTimeout -= SendRequest_OnTimeout;
                        _tcpConnection.OnProgress -= onProgress;

                        ReceiveResponseAsync(request);
                    }
                }
            };

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection requesting TcpConnection to send a stream.");
            _tcpConnection.SendAsync(stream, callback);
        }

        private void SendRequest_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnError -= SendRequest_OnError;
            _tcpConnection.OnTimeout -= SendRequest_OnTimeout;

            Logger.Network.Error("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection send timed-out.");
            if (OnTimeout != null) OnTimeout(this);
        }

        private void SendRequest_OnError(Tcp.TcpConnection sender, string message, Exception exception)
        {
            _tcpConnection.OnError -= SendRequest_OnError;
            _tcpConnection.OnTimeout -= SendRequest_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection encountered an error while sending data\r\nMessage: " + message, exception);
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
                {
                    Logger.Network.Error("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection timed-out while waiting for the 100-Continue response from the remote server.");
                    throw new TimeoutException("Timeout waiting for 100 continue status.");
                }

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
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() +
                    "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() +
                    "\r\nBytes Received: " + bytesReceived.ToString() +
                    "\r\nResponse Size: " + _responseBuilder.MessageSize.ToString() +
                    "\r\nPacket Size: " + packetSize.ToString() +
                    "\r\nHttpConnection reporting progress receiving from remote host.");
                if (OnProgress != null) OnProgress(this, direction, packetSize, 100, ((decimal)bytesReceived / (decimal)_responseBuilder.MessageSize));
            };

            _tcpConnection.OnProgress += onProgress;
            _tcpConnection.OnError += new Tcp.TcpConnection.ErrorDelegate(ReceiveResponseAsync_OnError);
            _tcpConnection.OnTimeout += new Tcp.TcpConnection.ConnectionDelegate(ReceiveResponseAsync_OnTimeout);

            callback = delegate(MessageBuilder sender, Message.Base message)
            {
                Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection receiving data from remote host completed.");
                if (OnComplete != null) OnComplete(this, (Response)message);
            };

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection beginning receiving of data from remote host.");
            _responseBuilder.ParseAndAttachToBody(_tcpConnection, callback);
        }

        private void ReceiveResponseAsync_OnTimeout(Tcp.TcpConnection sender)
        {
            _tcpConnection.OnError -= ReceiveResponseAsync_OnError;
            _tcpConnection.OnTimeout -= ReceiveResponseAsync_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection receiving of data timed-out.");
            if (OnTimeout != null) OnTimeout(this);
        }

        private void ReceiveResponseAsync_OnError(Tcp.TcpConnection sender, string message, Exception exception)
        {
            _tcpConnection.OnError -= ReceiveResponseAsync_OnError;
            _tcpConnection.OnTimeout -= ReceiveResponseAsync_OnTimeout;

            Logger.Network.Debug("HttpConnection ID: " + this.GetHashCode().ToString() + "\r\nTcpConnection ID: " + _tcpConnection.GetHashCode().ToString() + "\r\nHttpConnection encountered an error receiving data\r\nMessage: " + message, exception);
            if (OnError != null) OnError(this, message, exception);
        }
    }
}
