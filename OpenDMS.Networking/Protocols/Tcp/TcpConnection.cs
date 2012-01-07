using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class TcpConnection
    {
        public delegate void ConnectionDelegate(TcpConnection sender);
        public delegate void ErrorDelegate(TcpConnection sender, string message, Exception exception);
        public delegate void ProgressDelegate(TcpConnection sender, DirectionType direction, int packetSize);
        public delegate void AsyncCallback(TcpConnection sender, TcpConnectionAsyncEventArgs e);

        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ErrorDelegate OnError;
        public event ConnectionDelegate OnTimeout;
        public event ProgressDelegate OnProgress;

        private Socket _socket;
        private Stream _streamToSend;
        private ulong _bytesReceivedTotal = 0;
        private ulong _bytesSentTotal = 0;

        public IPEndPoint EndPoint { get; set; }
        public Params.Buffer ReceiveBufferSettings { get; set; }
        public Params.Buffer SendBufferSettings { get; set; }
        public bool IsConnected { get { return (_socket != null && _socket.Connected); } }
        public int BytesAvailable { get { return _socket.Available; } }
        public Socket Socket { get { return _socket; } }

        public TcpConnection(Params.Connection param)
        {
            EndPoint = param.EndPoint;
            ReceiveBufferSettings = param.ReceiveBuffer;
            SendBufferSettings = param.SendBuffer;

            Logger.Network.Debug("New TCP connection created for host " + EndPoint.Address.ToString() + " on port " + EndPoint.Port.ToString());
        }

        public void ConnectAsync()
        {
            SocketAsyncEventArgs socketArgs;

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSocket created.");
            }
            catch (Exception e)
            {
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while instantiating the socket.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception instantiating socket.", e);
                    return;
                }
                else throw;
            }

            try
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SendBufferSettings.Timeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveBufferSettings.Timeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, SendBufferSettings.Size);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ReceiveBufferSettings.Size);
            }
            catch (Exception e)
            {
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while settings socket options.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception settings socket options.", e);
                    return;
                }
                else throw;
            }

            lock (_socket)
            {
                socketArgs = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = EndPoint
                };
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
                socketArgs.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(SendBufferSettings.Timeout, Connect_Timeout));

                Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nConnecting to " + EndPoint.Address.ToString() + " on " + EndPoint.Port.ToString() + "...");

                try
                {
                    if (!_socket.ConnectAsync(socketArgs))
                    {
                        Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nConnectAsync completed synchronously.");
                        Connect_Completed(null, socketArgs);
                    }
                }
                catch (Exception e)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while connecting the socket.", e);
                    if (OnError != null) OnError(this, "Exception connecting socket.", e);
                    else throw;
                }
            }
        }

        private void CloseSocketAndTimeout()
        {
            lock (_socket)
            {
                Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nClosing socket.");
                try
                {
                    _socket.Close();
                }
                catch (Exception e)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while calling _socket.Close.", e);
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
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn unhandled exception was caught by Connection.CloseSocketAndTimeout in the OnTimeout event.", ex);
                    throw;
                }
            }
        }

        private void Connect_Timeout()
        {
            Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTimeout during connection.");
            CloseSocketAndTimeout();
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            TcpConnectionAsyncEventArgs args = (TcpConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(e.UserToken))
                return;

            Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSocket connected.");

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

        private bool TryStopTimeout(object obj)
        {
            if (obj.GetType() != typeof(TcpConnectionAsyncEventArgs))
                throw new ArgumentException("Argument must be of type ConnectionAsyncEventArgs");

            return TryStopTimeout((TcpConnectionAsyncEventArgs)obj);
        }

        private bool TryStopTimeout(TcpConnectionAsyncEventArgs userToken)
        {
            try
            {
                userToken.StopTimeout();
            }
            catch (Exception ex)
            {
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while stopping the timeout.", ex);
                if (OnError != null)
                {
                    OnError(this, "Exception while stopping timeout.", ex);
                    return false;
                }
                else throw;
            }

            return true;
        }

        public void DisconnectAsync()
        {
            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();

            if (_socket != null)
            {
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Close_Completed);
                lock (_socket)
                {
                    socketArgs.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(SendBufferSettings.Timeout, Close_Timeout));

                    Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nDisconnecting the socket and closing...");

                    try
                    {
                        if (!_socket.DisconnectAsync(socketArgs))
                        {
                            Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nDisconnectAsync completed synchronously.");
                            Close_Completed(null, socketArgs);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while disconnecting the socket.", e);
                        if (OnError != null) OnError(this, "Exception disconnecting socket.", e);
                        else throw;
                    }
                }
            }
        }

        private void Close_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!TryStopTimeout(e.UserToken))
                return;

            try
            {
                if (OnDisconnect != null) OnDisconnect(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn unhandled exception was caught by Connection.Close_Completed in the OnDisconnect event.", ex);
                throw;
            }

            lock (_socket)
            {
                Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nDisconnected and closing.");

                try
                {
                    _socket.Close();
                    _socket.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while calling _socket.Close.", ex);
                    if (OnError != null) OnError(this, "Exception calling _socket.Close.", ex);
                    else throw;
                }
            }
        }

        private void Close_Timeout()
        {
            Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTimeout during closing.");
            CloseSocketAndTimeout();
        }

        public void SendAsync(System.IO.Stream stream, AsyncCallback callback)
        {
            if (!IsConnected)
                throw new TcpConnectionException("Socket is closed or not ready");

            byte[] buffer;
            int bytesRead = 0;
            SocketAsyncEventArgs socketArgs;

            socketArgs = new SocketAsyncEventArgs();
            socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendAsyncStream_Completed);

            bytesRead = GetBytesFromStream(stream, out buffer, SendBufferSettings.Size);
            socketArgs.SetBuffer(buffer, 0, bytesRead);
            socketArgs.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(SendBufferSettings.Timeout, SendAsync_Timeout), stream, callback);

            lock (_socket)
            {
                try
                {
                    Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSending Packet:\r\n" + System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    if (!_socket.SendAsync(socketArgs))
                    {
                        Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSendAsync completed synchronously.");
                        SendAsyncStream_Completed(null, socketArgs);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while sending on socket.", ex);
                    if (OnError != null) OnError(this, "Exception sending on socket.", ex);
                    else throw;
                }
            }
        }

        private int GetBytesFromStream(System.IO.Stream stream, out byte[] buffer, int length)
        {
            buffer = new byte[length];
            return stream.Read(buffer, 0, length);
        }

        private void SendAsyncStream_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SendAsyncStream_Completed;

            byte[] buffer;
            int bytesRead = 0;
            TcpConnectionAsyncEventArgs userToken = (TcpConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;

            _bytesSentTotal += (ulong)e.BytesTransferred;
            if (OnProgress != null) OnProgress(this, DirectionType.Upload, e.BytesTransferred);

            bytesRead = GetBytesFromStream(userToken.Stream, out buffer, SendBufferSettings.Size);

            if (bytesRead <= 0)
            {
                if (userToken.AsyncCallback != null)
                {
                    userToken.BytesTransferred = e.BytesTransferred;
                    userToken.AsyncCallback(this, userToken);
                }
                return;
            }

            e.SetBuffer(buffer, 0, bytesRead);
            e.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(SendBufferSettings.Timeout, SendAsync_Timeout), userToken.Stream);

            lock (_socket)
            {
                try
                {
                    Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSending Packet:\r\n" + System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    if (!_socket.SendAsync(e))
                    {
                        Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSendAsyncStream_Completed completed synchronously.");
                        SendAsyncStream_Completed(null, e);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while sending on socket.", ex);
                    if (OnError != null) OnError(this, "Exception sending on socket.", ex);
                    else throw;
                }
            }
        }

        public void SendAsync(byte[] buffer, int offset, int length, AsyncCallback callback)
        {
            if (!IsConnected)
                throw new TcpConnectionException("Socket is closed or not ready");
            if (length > SendBufferSettings.Size)
                throw new TcpConnectionException("Length must be less than or equal to the size of the send buffer.");

            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();

            socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendAsync_Completed);
            socketArgs.SetBuffer(buffer, offset, length);
            socketArgs.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(SendBufferSettings.Timeout, SendAsync_Timeout), callback);
            
            lock (_socket)
            {
                try
                {
                    Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSending Packet:\r\n" + System.Text.Encoding.UTF8.GetString(buffer, 0, length - offset));
                    if (!_socket.SendAsync(socketArgs))
                    {
                        Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nSendAsync completed synchronously.");
                        SendAsync_Completed(null, socketArgs);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while sending on socket.", ex);
                    if (OnError != null) OnError(this, "Exception sending on socket.", ex);
                    else throw;
                }
            }
        }

        private void SendAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SendAsyncStream_Completed;
            TcpConnectionAsyncEventArgs userToken = (TcpConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;

            _bytesSentTotal += (ulong)e.BytesTransferred;
            if (OnProgress != null) OnProgress(this, DirectionType.Upload, e.BytesTransferred);

            if (userToken.AsyncCallback != null)
            {
                userToken.BytesTransferred = e.BytesTransferred;
                userToken.AsyncCallback(this, userToken);
            }
        }

        private void SendAsync_Timeout()
        {
            Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTimeout during sending request.");
            CloseSocketAndTimeout();
        }

        public void ReceiveAsync(AsyncCallback callback, object state)
        {
            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
            socketArgs.SetBuffer(new byte[ReceiveBufferSettings.Size], 0, ReceiveBufferSettings.Size);
            socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveAsync_Completed);
            socketArgs.UserToken = new TcpConnectionAsyncEventArgs(new Timeout(ReceiveBufferSettings.Timeout, ReceiveAsync_Timeout), callback) 
            { 
                UserToken = state 
            };

            try
            {
                if (!_socket.ReceiveAsync(socketArgs))
                {
                    Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nReceiveAsync completed synchronously.");
                    ReceiveAsync_Completed(null, socketArgs);
                }
            }
            catch (Exception e)
            {
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nAn exception occurred while receiving from the socket.", e);
                if (OnError != null) OnError(this, "Exception receiving from socket.", e);
                else throw;
            }
        }

        public void ReceiveAsync(AsyncCallback callback)
        {
            ReceiveAsync(callback, null);
        }

        private void ReceiveAsync_Timeout()
        {
            Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTimeout during receiving from socket.");
            CloseSocketAndTimeout();
        }

        private void ReceiveAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            TcpConnectionAsyncEventArgs userToken = (TcpConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;

            _bytesReceivedTotal += (ulong)e.BytesTransferred;

            Logger.Network.Debug("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nReceiving Packet of length " + e.BytesTransferred.ToString() + ":\r\n" + System.Text.Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred));

            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Download, e.BytesTransferred);
            }
            catch (Exception ex)
            {
                Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTcpConnection ID: " + this.GetHashCode().ToString() + "\r\nAn error occurred while reporting progress to higher level code.\r\nMessage: " + ex.Message, ex);
            }

            if (userToken.AsyncCallback != null)
            {
                userToken.BytesTransferred = e.BytesTransferred;
                userToken.Buffer = e.Buffer;
                userToken.Length = e.Count;
                try
                {
                    userToken.AsyncCallback(this, userToken);
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("Socket ID: " + _socket.GetHashCode().ToString() + "\r\nTcpConnection ID: " + this.GetHashCode().ToString() + "\r\nAn error occurred while reporting progress to higher level code.\r\nMessage: " + ex.Message, ex);
                }
            }
        }
    }
}
