using System;
using System.Net;
using System.Net.Sockets;

namespace Common.Http.Network
{
    public class HttpConnection
    {
        public delegate void ConnectionDelegate(HttpConnection sender);
        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ConnectionDelegate OnTimeout;
        public delegate void ErrorDelegate(HttpConnection sender, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void ProgressDelegate(HttpConnection sender, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void CompletionEvent(HttpConnection sender, Methods.HttpResponse response);
        public event CompletionEvent OnComplete;

        private HttpConnectionFactory _factory = null;
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

        public HttpConnection(HttpConnectionFactory factory, Uri uri) 
        {
            _factory = factory;
            _uri = uri;
        }

        public HttpConnection(HttpConnectionFactory factory, Uri uri, int sendTimeout, int receiveTimeout,
            int sendBufferSize, int receiveBufferSize)
            : this(factory, uri)
        {
            _sendTimeout = sendTimeout;
            _receiveTimeout = receiveTimeout;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        private bool TryCreateUserTokenAndTimeout(object token1, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            return TryCreateUserTokenAndTimeout(token1, null, milliseconds, out userToken, onTimeout);
        }

        private bool TryCreateUserTokenAndTimeout(object token1, object token2, int milliseconds,
            out AsyncUserToken userToken, Timeout.TimeoutEvent onTimeout)
        {
            userToken = null;

            try
            {
                userToken = new AsyncUserToken(token1, token2).StartTimeout(milliseconds, onTimeout);
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
            IPEndPoint remoteEP;

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

            remoteEP = new IPEndPoint(ipaddress, _uri.Port);

            _args = new SocketAsyncEventArgs();
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
                Logger.Network.Error("An unhandled exception was caught by HttpConnection.Connect_Completed in the OnConnect event.", ex);
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
                    Logger.Network.Error("An unhandled exception was caught by HttpConnection.CloseSocketAndTimeout in the OnTimeout event.", ex);
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
                Logger.Network.Error("An unhandled exception was caught by HttpConnection.Close_Completed in the OnDisconnect event.", ex);
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

        //public void SendRequestHeadersOnlyAsync(Methods.HttpRequest request)
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready");
            
        //    Logger.Network.Debug("Sending request headers...");

        //    _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequestHeadersOnly_Completed);
        //    _args.SetBuffer(new byte[_sendBufferSize], 0, _sendBufferSize);
        //    _args.UserToken = new AsyncUserToken(System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request)))
        //        .StartTimeout(_sendTimeout, new Timeout.TimeoutEvent(SendRequestHeadersOnly_Timeout));

        //    lock(_socket)
        //    {
        //        if (!_socket.SendAsync(_args))
        //        {
        //            SendRequestHeadersOnly_Completed(null, _args);
        //        }
        //    }
        //}

        //private void SendRequestHeadersOnly_Completed(object sender, SocketAsyncEventArgs e)
        //{
        //    _args.Completed -= SendRequestHeadersOnly_Completed;
        //    ((AsyncUserToken)_args.UserToken).StopTimeout();

        //    _bytesSentHeadersOnly += (ulong)e.BytesTransferred;
        //    _bytesSentTotal += (ulong)e.BytesTransferred;
            
        //    if(OnProgress != null) OnProgress(this, DirectionType.Upload, e.BytesTransferred);

        //    // If we are not done, we need to do it again
        //    if (e.BytesTransferred < ((byte[])e.UserToken).Length)
        //    {
        //        _args.SetBuffer(e.BytesTransferred, e.Buffer.Length - e.BytesTransferred);
        //        ((AsyncUserToken)_args.UserToken).StartTimeout(_sendTimeout,
        //            new Timeout.TimeoutEvent(SendRequestHeadersOnly_Timeout));

        //        lock(_socket)
        //        {
        //            if (!_socket.SendAsync(_args))
        //            {
        //                _args.Completed -= SendRequestHeadersOnly_Completed;
        //                SendRequestHeadersOnly_Completed(null, _args);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Logger.Network.Debug("Request headers were sent.");
        //        ReceiveResponseAsync();
        //    }
        //}

        //private void SendRequestHeadersOnly_Timeout()
        //{
        //    _args.Completed -= SendRequestHeadersOnly_Completed;
        //    Logger.Network.Error("Timeout during sending request headers.");
        //    CloseSocketAndTimeout();
        //}


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
            
            headers = (string)((AsyncUserToken)_args.UserToken).Token1;

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
                Logger.Network.Error("An unhandled exception was caught by HttpConnection.ReceiveResponse_Completed in the OnProgress event.", ex);
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
                Methods.HttpResponse response = new Methods.HttpResponse();
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
                    Logger.Network.Error("An unhandled exception was caught by HttpConnection.ReceiveResponse_Completed in the OnComplete event.", ex);
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
                Logger.Network.Error("An unhandled exception was caught by HttpConnection.Stream_OnProgress in the OnProgress event.", ex);
                throw;
            }
        }

        private void ReceiveResponse_Timeout()
        {
            _args.Completed -= ReceiveResponse_Completed;
            Logger.Network.Error("Timeout during receiving response headers.");
            CloseSocketAndTimeout();
        }

        public void SendRequest(Methods.HttpRequest request, System.IO.Stream stream)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready");

            AsyncUserToken userToken = null;
            Logger.Network.Debug("Sending request headers...");

            // Reset the stream position if we can
            if (stream.CanSeek)
            {
                stream.Position = 0;
                request.ContentLength = stream.Length.ToString();
            }
            else if (stream != null & Utilities.GetContentLength(request.Headers) <= 0)
                throw new HttpNetworkException("A stream was provide and the content length was not set.");
            else if (!stream.CanRead)
                throw new ArgumentException("The stream cannot be read.");


            _args.Completed += new EventHandler<SocketAsyncEventArgs>(SendRequest_Completed);
            _args.SetBuffer(new byte[_sendBufferSize], 0, _sendBufferSize);

            if (!TryCreateUserTokenAndTimeout(System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request)), 
                stream, _sendTimeout, out userToken, new Timeout.TimeoutEvent(SendRequest_Timeout)))
                return;

            _args.UserToken = userToken;

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

            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Upload, e.BytesTransferred);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpConnection.SendRequest_Completed in the OnProgress event.", ex);
                throw;
            }

            if (((AsyncUserToken)_args.UserToken).Token1 != null)
            {
                // Process Headers
                _bytesSentHeadersOnly += (ulong)e.BytesTransferred;
                _bytesSentTotal += (ulong)e.BytesTransferred;

                // If we are not done, we need to do it again
                if (e.BytesTransferred < ((byte[])e.UserToken).Length)
                {
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

        private string GetRequestHeader(Methods.HttpRequest request)
        {
            string str = request.RequestLine + "\r\n";
            str += "Host: " + _uri.Host + "\r\n";

            for (int i = 0; i < request.Headers.Count; i++)
                str += request.Headers.GetKey(i) + ": " + request.Headers[i] + "\r\n";

            str += "\r\n";

            return str;
        }

        //public void SendRequestHeaderOnly(Methods.HttpRequest request)
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready");

        //    int bytesSent = 0;

        //    Logger.Network.Debug("Sending request headers...");
        //    bytesSent = _socket.Send(System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request)));

        //    _bytesSentHeadersOnly += (ulong)bytesSent;
        //    _bytesSentTotal += (ulong)bytesSent;
        //    if (OnProgress != null) OnProgress(this, DirectionType.Upload, bytesSent);

        //    // Wait for response
        //    WaitForDataToArriveAtSocket(_receiveTimeout);

        //    if (_socket.Available <= 0)
        //    {
        //        Logger.Network.Debug("A timeout occurred while waiting on the server's response to the request headers.");
        //        throw new HttpNetworkTimeoutException("Timeout waiting on response.");
        //    }

        //    Logger.Network.Debug("Request headers have been sent and received by the server.");
        //}

        //public void SendRequestHeaderAndStream(Methods.HttpRequest request, System.IO.Stream stream)
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready");

        //    byte[] buffer = new byte[_sendBufferSize];
        //    int bytesRead = 0;
        //    int bytesSent = 0;

        //    if (stream != null)
        //    {
        //        Logger.Network.Debug("Found data to send, configuring use of 100-Continue.");
        //        request.Headers.Add("Expect", "100-Continue");
        //    }

        //    byte[] header = System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request).ToString());

        //    // Send headers
        //    Logger.Network.Debug("Sending request headers...");
        //    bytesSent = Send(header, 0, header.Length, _sendTimeout);

        //    _bytesSentHeadersOnly += (ulong)bytesSent;
        //    _bytesSentTotal += (ulong)bytesSent;
        //    if (OnProgress != null) OnProgress(this, DirectionType.Upload, bytesSent);

        //    // Do we need to wait for a 100-Continue response?
        //    if (!string.IsNullOrEmpty(request.Headers.Get("Expect")) &&
        //        request.Headers.Get("Expect") == "100-Continue")
        //    {
        //        Logger.Network.Debug("Waiting on the 100-Continue message...");
        //        WaitForDataToArriveAtSocket(_receiveTimeout);
        //        if (_socket.Available > 0)
        //        {
        //            // Read the 100-Continue response
        //            Methods.HttpResponse response = ReceiveResponseHeaders();
        //            if (response.ResponseCode != 100)
        //            {
        //                Logger.Network.Error("100-Continue server response was not received.");
        //                throw new HttpNetworkException("Reponse returned before data was sent, but it is not 100-continue.");
        //            }
        //        }
        //        else
        //        {
        //            Logger.Network.Error("The expected 100-Continue response was not received from the server within the " + _receiveTimeout.ToString() + "ms timeout period.");
        //            throw new HttpNetworkTimeoutException("A timeout occurred while waiting on the 100-Continue response.");
        //        }
        //    }

        //    Logger.Network.Debug("100-Continue server response was received, preparing to send data...");

        //    // Send payload
        //    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        //    {
        //        bytesSent = Send(buffer, 0, bytesRead, _sendTimeout);
        //        _bytesSentContentOnly += (ulong)bytesSent;
        //        _bytesSentTotal += (ulong)bytesSent;
        //        if (OnProgress != null) OnProgress(this, DirectionType.Upload, bytesSent);
        //    }
        //}

        //public void SendRequestHeaderAndStream(Methods.HttpRequest request, System.IO.Stream stream, Work.JobBase job)
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready");

        //    byte[] buffer = new byte[_sendBufferSize];
        //    int bytesRead = 0;
        //    int bytesSent = 0;

        //    if (stream != null)
        //    {
        //        Logger.Network.Debug("Found data to send, configuring use of 100-Continue.");
        //        request.Headers.Add("Expect", "100-Continue");
        //    }

        //    byte[] header = System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request).ToString());

        //    // Send headers
        //    Logger.Network.Debug("Sending request headers...");
        //    bytesSent = Send(header, 0, header.Length, _sendTimeout);

        //    _bytesSentHeadersOnly += (ulong)bytesSent;
        //    _bytesSentTotal += (ulong)bytesSent;
        //    if (OnProgress != null) OnProgress(this, DirectionType.Upload, bytesSent);

        //    if (job.IsCancelled) return;

        //    // Do we need to wait for a 100-Continue response?
        //    if (!string.IsNullOrEmpty(request.Headers.Get("Expect")) &&
        //        request.Headers.Get("Expect") == "100-Continue")
        //    {
        //        Logger.Network.Debug("Waiting on the 100-Continue message...");
        //        WaitForDataToArriveAtSocket(_receiveTimeout);
        //        if (_socket.Available > 0)
        //        {
        //            // Read the 100-Continue response
        //            Methods.HttpResponse response = ReceiveResponseHeaders();
        //            if (response.ResponseCode != 100)
        //            {
        //                Logger.Network.Error("100-Continue server response was not received.");
        //                throw new HttpNetworkException("Reponse returned before data was sent, but it is not 100-continue.");
        //            }
        //        }
        //        else
        //        {
        //            Logger.Network.Error("The expected 100-Continue response was not received from the server within the " + _receiveTimeout.ToString() + "ms timeout period.");
        //            throw new HttpNetworkTimeoutException("A timeout occurred while waiting on the 100-Continue response.");
        //        }
        //    }

        //    Logger.Network.Debug("100-Continue server response was received, preparing to send data...");

        //    if (job.IsCancelled) return;

        //    // Send payload
        //    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        //    {
        //        bytesSent = Send(buffer, 0, bytesRead, _sendTimeout);
        //        _bytesSentContentOnly += (ulong)bytesSent;
        //        _bytesSentTotal += (ulong)bytesSent;
        //        if (OnProgress != null) OnProgress(this, DirectionType.Upload, bytesSent);
        //        if (job.IsCancelled) return;
        //    }
        //}

        //private void WaitForDataToArriveAtSocket(int timeout)
        //{
        //    int counter = 0;
        //    // wait another timeout period for the response to arrive.
        //    while (!(_socket.Available > 0) && counter < (timeout / 100))
        //    {
        //        counter++;
        //        System.Threading.Thread.Sleep(100);
        //    }
        //}

        //private int Send(byte[] buffer, int offset, int length, int timeout)
        //{
        //    int startTickCount = Environment.TickCount;
        //    int sent = 0;

        //    while (sent < length)
        //    {
        //        if (Environment.TickCount > startTickCount + timeout)
        //            throw new HttpNetworkTimeoutException("Send timed out");

        //        try
        //        {
        //            sent += _socket.Send(buffer, offset + sent, length - sent, SocketFlags.None);
        //        }
        //        catch (SocketException e)
        //        {
        //            if (e.SocketErrorCode == SocketError.WouldBlock ||
        //                e.SocketErrorCode == SocketError.IOPending ||
        //                e.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
        //            {
        //                // Buffer might be full, lets give it a few milliseconds and try again
        //                // This will only keep retrying until the timeout passes.
        //                System.Threading.Thread.Sleep(30);
        //            }
        //            else if (e.SocketErrorCode == SocketError.TimedOut)
        //                throw new HttpNetworkTimeoutException(e.Message);
        //        }
        //    }

        //    return sent;
        //}

        

        //public Methods.HttpResponse ReceiveResponseHeaders()
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready.");

        //    Methods.HttpResponse response = new Methods.HttpResponse();
        //    string header = "";
        //    byte[] bytes;
        //    System.Text.RegularExpressions.MatchCollection matches;

        //    Logger.Network.Debug("Starting receiving of headers...");

        //    WaitForDataToArriveAtSocket(_receiveTimeout);

        //    // We must receive byte by byte to prevent reading any of the data stream
        //    // Perhaps this should be optimized to read chunks until the headers are done storing
        //    // any remainder in a temporary buffer which is first read once the ReceiveResponseBody is called
        //    bytes = new byte[1];
        //    while (_socket.Receive(bytes, 0, 1, SocketFlags.None) > 0)
        //    {
        //        header += System.Text.Encoding.ASCII.GetString(bytes, 0, 1);
        //        _bytesReceivedHeadersOnly++;
        //        _bytesReceivedTotal++;
        //        if (bytes[0] == '\n' && header.EndsWith("\r\n\r\n"))
        //            break;
        //    }
            
        //    matches = new System.Text.RegularExpressions.Regex("[^\r\n]+").Matches(header.TrimEnd('\r', '\n'));
        //    for (int n = 1; n < matches.Count; n++)
        //    {
        //        string[] strItem = matches[n].Value.Split(new char[] { ':' }, 2);
        //        if (strItem.Length > 1)
        //        {
        //            if (!strItem[0].Trim().ToLower().Equals("set-cookie"))
        //                response.Headers.Add(strItem[0].Trim(), strItem[1].Trim());
        //            else
        //                response.Headers.Add(strItem[0].Trim(), strItem[1].Trim());
        //        }
        //    }

        //    // set the response code
        //    if (matches.Count > 0)
        //    {
        //        try
        //        {
        //            string firstLine = matches[0].Value;
        //            int index1 = firstLine.IndexOf(" ");
        //            int index2 = firstLine.IndexOf(" ", index1 + 1);
        //            response.ResponseCode = Int32.Parse(firstLine.Substring(index1 + 1, index2 - index1 - 1));
        //        }
        //        catch (Exception e)
        //        {
        //            throw new HttpNetworkException("Response Code is missing from the response");
        //        }
        //    }
            
        //    Logger.Network.Debug("Receiving of headers is complete.");

        //    if (OnProgress != null) OnProgress(this, DirectionType.Download, (int)_bytesReceivedHeadersOnly);

        //    return response;
        //}

        //public void ReceiveResponseBody(Methods.HttpResponse response)
        //{
        //    if (!IsConnected)
        //        throw new HttpNetworkException("Socket is closed or not ready.");

        //    ulong contentLength = 0;

        //    Logger.Network.Debug("Allocating a network stream to the response content...");

        //    string chunkedHeader = Utilities.GetTransferEncoding(response.Headers);

        //    if (chunkedHeader != null &&
        //        chunkedHeader.ToLower() == "chunked")
        //    {
        //        throw new HttpNetworkException("Receiving of chunked data is not supported.");
        //    }
        //    else
        //    {
        //        if ((contentLength = Utilities.GetContentLength(response.Headers)) > 0)
        //        {
        //            response.Stream = new HttpNetworkStream(contentLength, _socket, System.IO.FileAccess.Read, false);
        //            response.Stream.OnDataReceived += new HttpNetworkStream.DataReceivedDelegate(Stream_OnDataReceived);
        //            Logger.Network.Debug("A network stream has been successfully attached to the response content.");
        //        }
        //        else
        //        {
        //            Logger.Network.Debug("There was no response content on which to attach a network stream.");
        //        }
        //    }

        //    return;
        //}

        //void Stream_OnDataReceived(ulong amount, ulong total)
        //{
        //    _bytesReceivedTotal += amount;
        //    _bytesReceivedContentOnly += amount;
        //    if (OnProgress != null) OnProgress(this, DirectionType.Download, (int)amount);
        //}
    }
}
