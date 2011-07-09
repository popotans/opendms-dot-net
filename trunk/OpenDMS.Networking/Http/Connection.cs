using System;
using System.Net;
using System.Net.Sockets;

namespace OpenDMS.Networking.Http
{
    public class Connection
    {
		#region Fields (19) 

        private SocketAsyncEventArgs _args = null;
        private ulong _bytesReceivedContentOnly = 0;
        private ulong _bytesReceivedHeadersOnly = 0;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentContentOnly = 0;
        private ulong _bytesSentHeadersOnly = 0;
        // Counters and such
        private ulong _bytesSentTotal = 0;
        private ulong _contentLengthRx = 0;
        private ulong _contentLengthTx = 0;
        private ConnectionManager _factory = null;
        private ulong _headersLengthRx = 0;
        private ulong _headersLengthTx = 0;
        private bool _isBusy = false;
        private int _receiveBufferSize = 8192;
        private int _receiveTimeout = 60000;
        private int _sendBufferSize = 8192;
        private int _sendTimeout = 60000;
        private Socket _socket = null;
        private Uri _uri = null;

		#endregion Fields 

		#region Constructors (2) 

        public Connection(ConnectionManager factory, Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
            : this(factory, uri)
        {
            _sendTimeout = sendTimeout;
            _receiveTimeout = receiveTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        public Connection(ConnectionManager factory, Uri uri) 
        {
            _factory = factory;
            _uri = uri;
        }

		#endregion Constructors 

		#region Properties (15) 

        public ulong BytesReceivedContentOnly { get { return _bytesReceivedContentOnly; } }

        public ulong BytesReceivedHeadersOnly { get { return _bytesReceivedHeadersOnly; } }

        public ulong BytesReceivedTotal { get { return _bytesReceivedTotal; } }

        public ulong BytesSentContentOnly { get { return _bytesSentContentOnly; } }

        public ulong BytesSentHeadersOnly { get { return _bytesSentHeadersOnly; } }

        public ulong BytesSentTotal { get { return _bytesSentTotal; } }

        public ulong ContentLength { get { return _contentLengthTx; } }

        public ulong HeadersLength { get { return _headersLengthTx; } }

        public bool IsBusy { get { return _isBusy; } set { _isBusy = value; } }

        public bool IsConnected { get { return (_socket != null && _socket.Connected); } }

        public decimal ReceivePercentComplete 
        { 
            get
            {
                if ((_headersLengthRx + _contentLengthRx) == 0)
                    return 0;
                return ((decimal)_bytesReceivedTotal / (decimal)(_headersLengthRx + _contentLengthRx)) * (decimal)100; 
            } 
        }

        public int ReceiveTimeout { get { return _receiveTimeout; } set { _receiveTimeout = value; } }

        public decimal SendPercentComplete 
        { 
            get 
            {
                if ((_headersLengthTx + _contentLengthTx) == 0)
                    return 0;
                return ((decimal)_bytesSentTotal / (decimal)(_headersLengthTx + _contentLengthTx)) * (decimal)100; 
            } 
        }

        public int SendTimeout { get { return _sendTimeout; } set { _sendTimeout = value; } }

        public Uri Uri { get { return _uri; } }

		#endregion Properties 

		#region Delegates and Events (10) 

		// Delegates (4) 

        public delegate void CompletionEvent(Connection sender, Methods.Response response);
        public delegate void ConnectionDelegate(Connection sender);
        public delegate void ErrorDelegate(Connection sender, string message, Exception exception);
        public delegate void ProgressDelegate(Connection sender, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
		// Events (6) 

        public event CompletionEvent OnComplete;

        public event ConnectionDelegate OnConnect;

        public event ConnectionDelegate OnDisconnect;

        public event ErrorDelegate OnError;

        public event ProgressDelegate OnProgress;

        public event ConnectionDelegate OnTimeout;

		#endregion Delegates and Events 

		#region Methods (23) 

		// Public Methods (3) 

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

        public void SendRequest(Methods.Request request, System.IO.Stream stream)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready");

            AsyncUserToken userToken = null;
            byte[] headers = null;
            Logger.Network.Debug("Sending request headers...");

            // Reset the stream position if we can
            if (stream != null)
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    _contentLengthTx = (ulong)stream.Length;
                    request.ContentLength = _contentLengthTx.ToString();
                }
                else if ((_contentLengthTx = Utilities.GetContentLength(request.Headers)) <= 0)
                    throw new HttpNetworkException("A stream was provided and the content length was not set.");
                else if (!stream.CanRead)
                    throw new ArgumentException("The stream cannot be read.");
            }


            _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);

            headers = System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request));

            _headersLengthTx = (ulong)headers.LongLength;

            if (!TryCreateUserTokenAndTimeout(request, new NetworkBuffer(headers), 
                stream, _sendTimeout, out userToken, new Timeout.TimeoutEvent(SendRequest_Timeout)))
                return;            

            _args.UserToken = userToken;
            if (userToken.NetworkBuffer.Length > _sendBufferSize)
            {
                byte[] newBuffer = new byte[_sendBufferSize];
                userToken.NetworkBuffer.CopyTo(newBuffer, 0, _sendBufferSize);
                _args.SetBuffer(newBuffer, 0, _sendBufferSize);
            }
            else
            {
                _args.SetBuffer(userToken.NetworkBuffer.Buffer, 0, userToken.NetworkBuffer.Length);
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
		// Private Methods (20) 

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
                    _socket.Dispose();
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

        private string GetRequestHeader(Methods.Request request)
        {
            string str = request.RequestLine + "\r\n";
            str += "Host: " + _uri.Host + "\r\n";

            for (int i = 0; i < request.Headers.Count; i++)
                str += request.Headers.GetKey(i) + ": " + request.Headers[i] + "\r\n";

            str += "\r\n";

            return str;
        }

        private void ReceiveResponse_ChunkedData(byte[] remainingBytes, Socket socket, Methods.Response response)
        {
            Tuple<string, Methods.Response> token;
            string responseBody = "";

            if (remainingBytes != null && remainingBytes.Length > 0)
            {
                responseBody += System.Text.Encoding.UTF8.GetString(remainingBytes);
                if (OnProgress != null) OnProgress(this, DirectionType.Download, remainingBytes.Length, 100, -1);
            }

            // Here we want to check to see if remaining bytes has all the data, if so, we do not want
            // to call the socket.ReceiveAsyc below because it will wait for data until timeout as no data will be sent.
            
            // So, how do we know if we have received the last chunk?
            // CouchDB sends its chunks with a 13,10,48,13,10,13,10 - \r\n0\r\n\r\n
            // This is also specified in the RFCs as it shows the size of the last chunk is 0
            if (responseBody.EndsWith("\r\n0\r\n\r\n"))
            {
                int chunkLength = 0;
                int packetSize = 0;
                int index;
                string newpacket = responseBody;

                responseBody = "";

                while ((index = newpacket.IndexOf("\r\n")) > 0)
                {
                    chunkLength = Utilities.ConvertHexToInt(newpacket.Substring(0, index));
                    newpacket = newpacket.Substring(index + 2);
                    if (chunkLength == 0)
                        break;
                    packetSize += chunkLength;
                    responseBody += newpacket.Substring(0, chunkLength);
                    newpacket = newpacket.Substring(chunkLength + 2);
                }

                _contentLengthRx += (ulong)packetSize;
                // Here, all info has been received
                response.Stream = new HttpNetworkStream(_contentLengthRx,
                    System.Text.Encoding.UTF8.GetBytes(responseBody),
                    _socket, System.IO.FileAccess.Read, false);
                if (OnComplete != null) OnComplete(this, response);
                return;
            }

            AsyncUserToken userToken;
            NetworkBuffer networkBuffer = new NetworkBuffer(new byte[_receiveBufferSize]);

            token = new Tuple<string, Methods.Response>(responseBody, response);
            if (!TryCreateUserTokenAndTimeout(networkBuffer, token, _receiveTimeout, out userToken, 
                new Timeout.TimeoutEvent(ReceiveResponse_ChunkedData_Timeout)))
                return;

            _args = new SocketAsyncEventArgs();
            _args.UserToken = userToken;
            _args.SetBuffer(networkBuffer.Buffer, 0, networkBuffer.Length);
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_ChunkedData_Completed);

            try
            {
                if (!socket.ReceiveAsync(_args))
                {
                    Logger.Network.Debug("ReceiveAsync completed synchronously.");
                    ReceiveResponse_ChunkedData_Completed(null, _args);
                }
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while receiving from the socket.", e);
                if (OnError != null) OnError(this, "Exception receiving from socket.", e);
                else throw;
            }
        }

        private void ReceiveResponse_ChunkedData_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= ReceiveResponse_ChunkedData_Completed;

            if (!TryStopTimeout(e.UserToken))
                return;

            AsyncUserToken userToken;
            string newpacket;
            Methods.Response response;
            Tuple<string, Methods.Response> token;
            string responseBody;
            int chunkLength = 0;
            int packetSize = 0;
            int index;

            try
            {
                newpacket = System.Text.Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
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

            userToken = (AsyncUserToken)e.UserToken;
            token = (Tuple<string, Methods.Response>)userToken.Token;
            responseBody = token.Item1;
            response = token.Item2;
            
            while ((index = newpacket.IndexOf("\r\n")) > 0)
            {
                chunkLength = Utilities.ConvertHexToInt(newpacket.Substring(0, index));
                newpacket = newpacket.Substring(index + 2);
                if (chunkLength == 0)
                    break;
                packetSize += chunkLength;
                responseBody += newpacket.Substring(0, chunkLength);
                newpacket = newpacket.Substring(chunkLength + 2);
            }

            _contentLengthRx += (ulong)packetSize;
            if (OnProgress != null) OnProgress(this, DirectionType.Download, packetSize, 100, -1);

            if (chunkLength > 0)
            {
                token = new Tuple<string, Methods.Response>(responseBody, response);
                if (!TryCreateUserTokenAndTimeout(userToken.Request, userToken.NetworkBuffer, token, _receiveTimeout, out userToken,
                    new Timeout.TimeoutEvent(ReceiveResponse_ChunkedData_Timeout)))
                    return;

                e.SetBuffer(userToken.NetworkBuffer.Buffer, 0, userToken.NetworkBuffer.Length);
                e.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_ChunkedData_Completed);
            }
            else
            {
                response.Stream = new HttpNetworkStream(_contentLengthRx,
                    System.Text.Encoding.UTF8.GetBytes(responseBody),
                    _socket, System.IO.FileAccess.Read, false);
                if (OnComplete != null) OnComplete(this, response);
            }
        }

        private void ReceiveResponse_ChunkedData_Timeout()
        {
            _args.Completed -= ReceiveResponse_ChunkedData_Completed;
            Logger.Network.Error("Timeout during receiving chunked response.");
            CloseSocketAndTimeout();
        }

        private void ReceiveResponse_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= ReceiveResponse_Completed;

            if (!TryStopTimeout(e.UserToken))
                return;

            int index = -1;
            string headers;
            string newpacket;
            Methods.Request request;
            AsyncUserToken userToken;

            userToken = (AsyncUserToken)e.UserToken;
            request = userToken.Request;
            userToken = null;
            
            headers = (string)((AsyncUserToken)e.UserToken).Token;

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

            if ((index = newpacket.IndexOf("\r\n\r\n")) < 0)
            {
                // End sequence \r\n\r\n is not found, we need to do this receiving again
                headers += newpacket;
                e.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_Completed);

                if (!TryCreateUserTokenAndTimeout(userToken.Request, headers, _receiveTimeout, out userToken,
                    new Timeout.TimeoutEvent(ReceiveResponse_Timeout)))
                    return;

                e.UserToken = userToken;

                try
                {
                    if (!_socket.ReceiveAsync(e))
                    {
                        Logger.Network.Debug("ReceiveAsync completed synchronously.");
                        ReceiveResponse_Completed(null, e);
                        return;
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
                Methods.Response response = new Methods.Response(request);
                System.Text.RegularExpressions.MatchCollection matches;
                string transferEncoding = null;
                //ulong contentLength = 0;
                
                headers += newpacket.Substring(0, index);
                remainingBytes = System.Text.Encoding.ASCII.GetBytes(newpacket.Substring(index + 4));

                _headersLengthRx = (ulong)headers.Length;
                // We do it here
                _bytesReceivedHeadersOnly = _headersLengthRx;
                _bytesReceivedTotal = _headersLengthRx;

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
                    ReceiveResponse_ChunkedData(remainingBytes, _socket, response);
                    return;
                }
                else
                {
                    if ((_contentLengthRx = Utilities.GetContentLength(response.Headers)) > 0)
                    {
                        response.Stream = new HttpNetworkStream(_contentLengthRx, remainingBytes, _socket, System.IO.FileAccess.Read, false);
                        response.Stream.OnProgress += new HttpNetworkStream.ProgressDelegate(Stream_OnProgress);
                        Logger.Network.Debug("A network stream has been successfully attached to the response body.");
                    }
                }

                try
                {
                    if (OnProgress != null && e.BytesTransferred > 0)
                        OnProgress(this, DirectionType.Download, e.BytesTransferred, SendPercentComplete, ReceivePercentComplete);
                }
                catch (Exception ex)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by Connection.ReceiveResponse_Completed in the OnProgress event.", ex);
                    throw;
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
                    if (OnError != null) OnError(this, ex.Message, ex);
                }
            }
        }

        private void ReceiveResponse_Timeout()
        {
            _args.Completed -= ReceiveResponse_Completed;
            Logger.Network.Error("Timeout during receiving response headers.");
            CloseSocketAndTimeout();
        }

        private void ReceiveResponseAsync(Methods.Request request)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready.");

            AsyncUserToken userToken = null;
            string headers = "";
            byte[] buffer = new byte[_receiveBufferSize];

            Logger.Network.Debug("Receiving response headers...");

            if (!TryCreateUserTokenAndTimeout(request, headers, _receiveTimeout, out userToken,
                new Timeout.TimeoutEvent(ReceiveResponse_Timeout)))
                return;

            _args.UserToken = userToken;
            _args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveResponse_Completed);
            _args.SetBuffer(buffer, 0, buffer.Length);
            
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

        private void SendRequest_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncUserToken userToken = null;
            e.Completed -= SendRequest_Completed;

            userToken = (AsyncUserToken)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;
            
            try
            {
                if (OnProgress != null) 
                    OnProgress(this, DirectionType.Upload, e.BytesTransferred, SendPercentComplete, ReceivePercentComplete);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.SendRequest_Completed in the OnProgress event.", ex);
                throw;
            }

            if (userToken.NetworkBuffer != null)
            {
                // Process Headers
                _bytesSentHeadersOnly += (ulong)e.BytesTransferred;
                _bytesSentTotal += (ulong)e.BytesTransferred;

                // If we are not done, we need to do it again
                if (e.BytesTransferred < userToken.NetworkBuffer.Length)
                {
                    // Make a new buffer for the remaining bytes of the Token1
                    byte[] newUserTokenBuffer = new byte[userToken.NetworkBuffer.Length - e.BytesTransferred];
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);
                    e.SetBuffer(e.BytesTransferred, e.Buffer.Length - e.BytesTransferred);

                    try
                    {
                        userToken.StartTimeout(_sendTimeout,
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

                    if (userToken.Token != null)
                    {
                        Logger.Network.Debug("Sending request body.");
                        byte[] buffer = new byte[_sendBufferSize];
                        int bytesRead = 0;

                        bytesRead = ((System.IO.Stream)userToken.Token).Read(buffer, 0, buffer.Length);
                        e.SetBuffer(buffer, 0, bytesRead);
                        e.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);

                        if (!TryCreateUserTokenAndTimeout(userToken.Request, null, userToken.Token,
                            _sendTimeout, out userToken, new Timeout.TimeoutEvent(SendRequest_Timeout)))
                            return;

                        e.UserToken = userToken;
                    }
                    else
                    {
                        ReceiveResponseAsync(userToken.Request);
                        return;
                    }
                }
            }
            else if (userToken.Token != null)
            { 
                // Flow falls here when the Token2 (stream) is not null
                //System.IO.Stream stream = (System.IO.Stream)((AsyncUserToken)_args.UserToken).Token2;
                byte[] buffer = new byte[_sendBufferSize];
                int bytesRead = 0;
                _bytesSentContentOnly += (ulong)e.BytesTransferred;
                _bytesSentTotal += (ulong)e.BytesTransferred;

                if (((System.IO.Stream)userToken.Token).Position == ((System.IO.Stream)userToken.Token).Length)
                {
                    ReceiveResponseAsync(userToken.Request);
                    return;
                }
                else
                {
                    bytesRead = ((System.IO.Stream)userToken.Token).Read(buffer, 0, buffer.Length);
                    e.SetBuffer(buffer, 0, bytesRead);
                    //e.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);

                    try
                    {
                        userToken.StartTimeout(_sendTimeout,
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
                ReceiveResponseAsync(userToken.Request);
                return;
            }

            lock (_socket)
            {
                try
                {
                    if (!_socket.SendAsync(e))
                    {
                        Logger.Network.Debug("SendAsync completed synchronously.");
                        SendRequest_Completed(null, e);
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

        private void Stream_OnProgress(HttpNetworkStream sender, DirectionType direction, int packetSize)
        {
            _bytesReceivedTotal += (ulong)packetSize;
            _bytesReceivedContentOnly += (ulong)packetSize;

            try
            {
                if (OnProgress != null) 
                    OnProgress(this, DirectionType.Download, packetSize, SendPercentComplete, ReceivePercentComplete);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by Connection.Stream_OnProgress in the OnProgress event.", ex);
                throw;
            }
        }

        private bool TryCreateUserTokenAndTimeout(NetworkBuffer networkBuffer, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(null, networkBuffer, null, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(Methods.Request request, NetworkBuffer networkBuffer, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(request, networkBuffer, null, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(null, null, token2, milliseconds, out userToken, onTimeout);
        }
        
        private bool TryCreateUserTokenAndTimeout(Methods.Request request, object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(request, null, token2, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(NetworkBuffer networkBuffer, object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(null, null, token2, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(Methods.Request request, NetworkBuffer networkBuffer, object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            userToken = null;

            try
            {
                userToken = new AsyncUserToken(request, networkBuffer, token2).StartTimeout(milliseconds, onTimeout);
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

		#endregion Methods 
    }
}
