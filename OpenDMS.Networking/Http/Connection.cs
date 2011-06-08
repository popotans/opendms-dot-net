using System;
using System.Net;
using System.Net.Sockets;

namespace OpenDMS.Networking.Http
{
    public class Connection
    {
        public delegate void ConnectionDelegate(Connection sender);
        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ConnectionDelegate OnTimeout;
        public delegate void ErrorDelegate(Connection sender, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void ProgressDelegate(Connection sender, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void CompletionEvent(Connection sender, Methods.Response response);
        public event CompletionEvent OnComplete;

        private ConnectionManager _factory = null;
        private Uri _uri = null;
        private Socket _socket = null;
        private int _sendTimeout = 60000;
        private int _receiveTimeout = 60000;
        private int _sendBufferSize = 8192;
        private int _receiveBufferSize = 8192;
        private bool _isBusy = false;

        public Uri Uri { get { return _uri; } }
        public int SendTimeout { get { return _sendTimeout; } set { _sendTimeout = value; } }
        public int ReceiveTimeout { get { return _receiveTimeout; } set { _receiveTimeout = value; } }
        public bool IsBusy { get { return _isBusy; } set { _isBusy = value; } }
        public bool IsConnected { get { return (_socket != null && _socket.Connected); } }

        // Counters and such
        private ulong _bytesSentTotal = 0;
        private ulong _bytesSentHeadersOnly = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedContentOnly = 0;

        private SocketAsyncEventArgs _args = null;

        public ulong BytesSentTotal { get { return _bytesSentTotal; } }
        public ulong BytesSentHeadersOnly { get { return _bytesSentHeadersOnly; } }
        public ulong BytesSentContentOnly { get { return _bytesSentContentOnly; } }
        public ulong BytesReceivedTotal { get { return _bytesReceivedTotal; } }
        public ulong BytesReceivedHeadersOnly { get { return _bytesReceivedHeadersOnly; } }
        public ulong BytesReceivedContentOnly { get { return _bytesReceivedContentOnly; } }

        public Connection(ConnectionManager factory, Uri uri) 
        {
            _factory = factory;
            _uri = uri;
        }

        public Connection(ConnectionManager factory, Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
            : this(factory, uri)
        {
            _sendTimeout = sendTimeout;
            _receiveTimeout = receiveTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        private bool TryCreateUserTokenAndTimeout(NetworkBuffer networkBuffer, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(networkBuffer, null, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(null, token2, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(NetworkBuffer networkBuffer, object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            userToken = null;

            try
            {
                userToken = new AsyncUserToken(networkBuffer, token2).StartTimeout(milliseconds, onTimeout);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while starting the timeout.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception while starting timeout.", e);
                    return false;
                }
                else throw;
            }

            return true;
        }

        private bool TryStopTimeout(object obj)
        {
            if (obj.GetType() != typeof(AsyncUserToken))
                throw new ArgumentException("Argument must be of type AsyncUserToken");

            return TryStopTimeout((AsyncUserToken)obj);
        }

        private bool TryStopTimeout(AsyncUserToken userToken)
        {
            try
            {
                userToken.StopTimeout();
            }
            catch (Exception ex)
            {
                Logger.Network.Error("An exception occurred while stopping the timeout.", ex);
                if (OnError != null)
                {
                    OnError(this, "Exception while stopping timeout.", ex);
                    return false;
                }
                else throw;
            }

            return true;
        }

        public void ConnectAsync()
        {
            AsyncUserToken userToken = null;
            IPAddress ipaddress;

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while instantiating the socket.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception instantiating socket.", e);
                    return;
                }
                else throw;
            }

            if (!IPAddress.TryParse(_uri.Host, out ipaddress))
                ipaddress = Dns.GetHostEntry(_uri.Host).AddressList[0];

            _args = new SocketAsyncEventArgs();
            _args.RemoteEndPoint = new IPEndPoint(ipaddress, _uri.Port);
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);

            try
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _sendTimeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _receiveTimeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _sendBufferSize);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while settings socket options.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception settings socket options.", e);
                    return;
                }
                else throw;
            }

            lock (_socket)
            {
                if (!TryCreateUserTokenAndTimeout(null, _sendTimeout, out userToken, 
                    new Timeout.TimeoutEvent(Connect_Timeout)))
                    return;

                _args.UserToken = userToken;

                Logger.Network.Debug("Connecting to " + _uri.ToString() + "...");

                try
                {
                    if (!_socket.ConnectAsync(_args))
                    {
                        Logger.Network.Debug("ConnectAsync completed synchronously.");
                        Connect_Completed(null, _args);
                    }
                }
                catch (Exception e)
                {
                    Logger.Network.Error("An exception occurred while connecting the socket.", e);
                    if (OnError != null) OnError(this, "Exception connecting socket.", e);
                    else throw;
                }
            }
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            _args.Completed -= Connect_Completed;

            if (!TryStopTimeout(_args.UserToken))
                return;

            Logger.Network.Debug("Connected successfully.");

            try
            {
                if (OnConnect != null) OnConnect(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.Connect_Completed in the OnConnect event.", ex);
                throw;
            }
        }

        private void Connect_Timeout()
        {
            _args.Completed -= Connect_Completed;
            Logger.Network.Error("Timeout during connection.");
            CloseSocketAndTimeout();
        }

        private void CloseSocketAndTimeout()
        {
            lock (_socket)
            {
                Logger.Network.Debug("Closing socket.");
                try
                {
                    _socket.Close();
                }
                catch (Exception e)
                {
                    Logger.Network.Error("An exception occurred while calling _socket.Close.", e);
                    if (OnError != null) OnError(this, "Exception calling _socket.Close.", e);
                    else throw;
                }

                try
                {
                    if (OnTimeout != null) OnTimeout(this);
                }
                catch (Exception ex)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by Connection.CloseSocketAndTimeout in the OnTimeout event.", ex);
                    throw;
                }
            }
        }

        public void CloseAsync()
        {
            AsyncUserToken userToken = null;

            if (_socket != null)
            {
                _args.Completed += new EventHandler<SocketAsyncEventArgs>(Close_Completed);
                lock (_socket)
                {
                    if (!TryCreateUserTokenAndTimeout(null, _sendTimeout, out userToken,
                        new Timeout.TimeoutEvent(Close_Timeout)))
                        return;

                    _args.UserToken = userToken;

                    Logger.Network.Debug("Disconnecting the socket and closing...");

                    try
                    {
                        if (!_socket.DisconnectAsync(_args))
                        {
                            Logger.Network.Debug("CloseAsync completed synchronously.");
                            Close_Completed(null, _args);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Network.Error("An exception occurred while disconnecting the socket.", e);
                        if (OnError != null) OnError(this, "Exception disconnecting socket.", e);
                        else throw;
                    }
                }
            }
        }

        private void Close_Completed(object sender, SocketAsyncEventArgs e)
        {
            _args.Completed -= Close_Completed;

            if (!TryStopTimeout(_args.UserToken))
                return;

            try
            {
                if (OnDisconnect != null) OnDisconnect(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.Close_Completed in the OnDisconnect event.", ex);
                throw;
            }

            lock (_socket)
            {
                Logger.Network.Debug("Disconnected and closing.");

                try
                {
                    _socket.Close();
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("An exception occurred while calling _socket.Close.", ex);
                    if (OnError != null) OnError(this, "Exception calling _socket.Close.", ex);
                    else throw;
                }
            }
        }

        private void Close_Timeout()
        {
            _args.Completed -= Close_Completed;
            Logger.Network.Error("Timeout during closing.");
            CloseSocketAndTimeout();
        }
        
        private void ReceiveResponseAsync()
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready.");

            AsyncUserToken userToken = null;
            string headers = "";

            Logger.Network.Debug("Receiving response headers...");

            if (!TryCreateUserTokenAndTimeout(headers, _receiveTimeout, out userToken,
                new Timeout.TimeoutEvent(ReceiveResponse_Timeout)))
                return;

            _args.UserToken = userToken;
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_Completed);
            
            lock (_socket)
            {
                try
                {
                    if (!_socket.ReceiveAsync(_args))
                    {
                        Logger.Network.Debug("ReceiveAsync completed synchronously.");
                        ReceiveResponse_Completed(null, _args);
                    }
                }
                catch (Exception e)
                {
                    Logger.Network.Error("An exception occurred while receiving from the socket.", e);
                    if (OnError != null) OnError(this, "Exception receiving from socket.", e);
                    else throw;
                }
            }
        }    

        private void ReceiveResponse_Completed(object sender, SocketAsyncEventArgs e)
        {
            _args.Completed -= ReceiveResponse_Completed;

            if (!TryStopTimeout(_args.UserToken))
                return;

            int index = -1;
            string headers;
            string newpacket;
            AsyncUserToken userToken = null;
            
            headers = (string)((AsyncUserToken)_args.UserToken).Token;

            try
            {
                newpacket = System.Text.Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred);
            }
            catch (Exception ex)
            {
                Logger.Network.Error("An exception occurred while getting a string from the buffer.", ex);
                if (OnError != null)
                {
                    OnError(this, "Exception while getting a string from the buffer.", ex);
                    return;
                }
                else throw;
            }

            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Download, e.BytesTransferred);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.ReceiveResponse_Completed in the OnProgress event.", ex);
                throw;
            }

            if ((index = newpacket.IndexOf("\r\n\r\n")) < 0)
            {
                // End sequence \r\n\r\n is not found, we need to do this receiving again
                headers += newpacket;
                _args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_Completed);

                if (!TryCreateUserTokenAndTimeout(headers, _receiveTimeout, out userToken,
                    new Timeout.TimeoutEvent(ReceiveResponse_Timeout)))
                    return;

                _args.UserToken = userToken;

                try
                {
                    if (!_socket.ReceiveAsync(_args))
                    {
                        Logger.Network.Debug("ReceiveAsync completed synchronously.");
                        ReceiveResponse_Completed(null, _args);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("An exception occurred while receiving from socket.", ex);
                    if (OnError != null) OnError(this, "Exception receiving from socket.", ex);
                    else throw;
                }
            }
            else
            {
                // bytes that are not header
                byte[] remainingBytes;
                // Push the remaining bytes into a stream
                System.IO.MemoryStream ms; 
                Methods.Response response = new Methods.Response();
                System.Text.RegularExpressions.MatchCollection matches;
                string transferEncoding = null;
                ulong contentLength = 0;
                
                headers += newpacket.Substring(0, index);
                remainingBytes = System.Text.Encoding.ASCII.GetBytes(newpacket.Substring(index + 8));
                ms = new System.IO.MemoryStream(remainingBytes);

                // Grab the headers from the response
                matches = new System.Text.RegularExpressions.Regex("[^\r\n]+").Matches(headers.TrimEnd('\r', '\n'));
                for (int n = 1; n < matches.Count; n++)
                {
                    string[] strItem = matches[n].Value.Split(new char[] { ':' }, 2);
                    if (strItem.Length > 1)
                    {
                        if (!strItem[0].Trim().ToLower().Equals("set-cookie"))
                            response.Headers.Add(strItem[0].Trim(), strItem[1].Trim());
                        else
                        {
                            // cookies can be implemented here, but for now they are not implemented,
                            // so if the server wants to use them, lets tell the programmer
                            throw new NotImplementedException("Cookies are not implemented, implement them because the server wants to use them!");
                        }
                    }
                }

                // set the response code
                if (matches.Count > 0)
                {
                    try
                    {
                        string firstLine = matches[0].Value;
                        int index1 = firstLine.IndexOf(" ");
                        int index2 = firstLine.IndexOf(" ", index1 + 1);
                        response.ResponseCode = Int32.Parse(firstLine.Substring(index1 + 1, index2 - index1 - 1));
                    }
                    catch (Exception ex)
                    {
                        throw new HttpNetworkException("Response Code is missing from the response", ex);
                    }
                }

                transferEncoding = Utilities.GetTransferEncoding(response.Headers);

                if (transferEncoding != null &&
                    transferEncoding.ToLower() == "chunked")
                {
                    // Currently, chunked encoding is not supported, tell the programmer.
                    throw new NotImplementedException("Receiving of chunked data is not supported, implement it because the server wants to use it.");
                }
                else
                {
                    if ((contentLength = Utilities.GetContentLength(response.Headers)) > 0)
                    {
                        response.Stream = new HttpNetworkStream(contentLength, _socket, System.IO.FileAccess.Read, false);
                        response.Stream.OnProgress += new HttpNetworkStream.ProgressDelegate(Stream_OnProgress);
                        Logger.Network.Debug("A network stream has been successfully attached to the response body.");
                    }
                }

                Logger.Network.Debug("Received response.");

                try
                {
                    if (OnComplete != null) OnComplete(this, response);
                }
                catch (Exception ex)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by Connection.ReceiveResponse_Completed in the OnComplete event.", ex);
                    throw;
                }
            }
        }

        private void Stream_OnProgress(HttpNetworkStream sender, DirectionType direction, int packetSize)
        {
            _bytesReceivedTotal += (ulong)packetSize;
            _bytesReceivedContentOnly += (ulong)packetSize;

            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Download, packetSize);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.Stream_OnProgress in the OnProgress event.", ex);
                throw;
            }
        }

        private void ReceiveResponse_Timeout()
        {
            _args.Completed -= ReceiveResponse_Completed;
            Logger.Network.Error("Timeout during receiving response headers.");
            CloseSocketAndTimeout();
        }

        public void SendRequest(Methods.Request request, System.IO.Stream stream)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready");

            AsyncUserToken userToken = null;
            Logger.Network.Debug("Sending request headers...");

            // Reset the stream position if we can
            if (stream != null)
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    request.ContentLength = stream.Length.ToString();
                }
                else if (Utilities.GetContentLength(request.Headers) <= 0)
                    throw new HttpNetworkException("A stream was provide and the content length was not set.");
                else if (!stream.CanRead)
                    throw new ArgumentException("The stream cannot be read.");
            }


            _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);

            if (!TryCreateUserTokenAndTimeout(System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request)), 
                stream, _sendTimeout, out userToken, new Timeout.TimeoutEvent(SendRequest_Timeout)))
                return;            

            _args.UserToken = userToken;
            if (((byte[])userToken.Token.Token1).Length > _sendBufferSize)
            {
                byte[] newBuffer = new byte[_sendBufferSize];
                Buffer.BlockCopy((byte[])userToken.Token1, 0, newBuffer, 0, _sendBufferSize);
                _args.SetBuffer(newBuffer, 0, _sendBufferSize);
            }

            lock (_socket)
            {
                try
                {
                    if (!_socket.SendAsync(_args))
                    {
                        Logger.Network.Debug("SendAsync completed synchronously.");
                        SendRequest_Completed(null, _args);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("An exception occurred while sending on socket.", ex);
                    if (OnError != null) OnError(this, "Exception sending on socket.", ex);
                    else throw;
                }
            }
        }

        private void SendRequest_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncUserToken userToken = null;
            _args.Completed -= SendRequest_Completed;

            if (!TryStopTimeout(_args.UserToken))
                return;

            userToken = (AsyncUserToken)e.UserToken;

            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Upload, e.BytesTransferred);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.SendRequest_Completed in the OnProgress event.", ex);
                throw;
            }

            if (userToken.Token1 != null)
            {
                // Process Headers
                _bytesSentHeadersOnly += (ulong)e.BytesTransferred;
                _bytesSentTotal += (ulong)e.BytesTransferred;

                // If we are not done, we need to do it again
                if (e.BytesTransferred < ((byte[])e.UserToken).Length)
                {
                    // Make a new buffer for the remaining bytes of the Token1
                    byte[] newUserTokenBuffer = new byte[((byte[])userToken.Token1).Length - e.BytesTransferred];

                    _args.SetBuffer(e.BytesTransferred, e.Buffer.Length - e.BytesTransferred);

                    try
                    {
                        ((AsyncUserToken)_args.UserToken).StartTimeout(_sendTimeout,
                            new Timeout.TimeoutEvent(SendRequest_Timeout));
                    }
                    catch (Exception ex)
                    {
                        Logger.Network.Error("An exception occurred while starting the timeout.", ex);
                        if (OnError != null)
                        {
                            OnError(this, "Exception while starting timeout.", ex);
                            return;
                        }
                        else throw;
                    }
                }
                else
                {
                    Logger.Network.Debug("Request headers were sent.");
                    Logger.Network.Debug("Sending request body.");

                    _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);
                    _args.SetBuffer(0, _args.Buffer.Length); // Frees up the entire buffer

                    if (!TryCreateUserTokenAndTimeout(null, ((AsyncUserToken)_args.UserToken).Token2,
                        _sendTimeout, out userToken, new Timeout.TimeoutEvent(SendRequest_Timeout)))
                        return;

                    _args.UserToken = userToken;
                }
            }
            else if (((AsyncUserToken)_args.UserToken).Token2 != null)
            { 
                // Flow falls here when the Token2 (stream) is not null
                System.IO.Stream stream = (System.IO.Stream)((AsyncUserToken)_args.UserToken).Token2;
                byte[] buffer = _args.Buffer;
                int bytesRead = 0;
                _bytesSentContentOnly += (ulong)e.BytesTransferred;
                _bytesSentTotal += (ulong)e.BytesTransferred;

                if (stream.Position == stream.Length)
                {
                    ReceiveResponseAsync();
                    return;
                }
                else
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    _args.SetBuffer(buffer, 0, bytesRead);
                    _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);

                    try
                    {
                        ((AsyncUserToken)_args.UserToken).StartTimeout(_sendTimeout,
                            new Timeout.TimeoutEvent(SendRequest_Timeout));
                    }
                    catch (Exception ex)
                    {
                        Logger.Network.Error("An exception occurred while starting the timeout.", ex);
                        if (OnError != null)
                        {
                            OnError(this, "Exception while starting timeout.", ex);
                            return;
                        }
                        else throw;
                    }
                }
            }
            else
            {
                ReceiveResponseAsync();
                return;
            }

            lock (_socket)
            {
                try
                {
                    if (!_socket.SendAsync(_args))
                    {
                        Logger.Network.Debug("SendAsync completed synchronously.");
                        SendRequest_Completed(null, _args);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("An exception occurred while sending on socket.", ex);
                    if (OnError != null) OnError(this, "Exception sending on socket.", ex);
                    else throw;
                }
            }
        }

        private void SendRequest_Timeout()
        {
            _args.Completed -= SendRequest_Completed;
            Logger.Network.Error("Timeout during sending request.");
            CloseSocketAndTimeout();
        }

        private string GetRequestHeader(Methods.Request request)
        {
            string str = request.RequestLine + "\r\n";
            str += "Host: " + _uri.Host + "\r\n";

            for (int i = 0; i < request.Headers.Count; i++)
                str += request.Headers.GetKey(i) + ": " + request.Headers[i] + "\r\n";

            str += "\r\n";

            return str;
        }
    }
}
