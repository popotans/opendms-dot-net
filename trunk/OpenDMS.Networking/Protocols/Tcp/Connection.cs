using System;
using System.Net;
using System.Net.Sockets;

namespace OpenDMS.Networking.Protocols.Tcp
{
    public class Connection
    {
        public delegate void ConnectionDelegate(Connection sender);
        public delegate void ReceiveDelegate(Connection sender, byte[] buffer, int offset, int length);
        public delegate void ErrorDelegate(Connection sender, string message, Exception exception);

        public event ConnectionDelegate OnConnect;
        public event ConnectionDelegate OnDisconnect;
        public event ConnectionDelegate OnSendComplete;
        public event ReceiveDelegate OnReceiveComplete;
        public event ErrorDelegate OnError;
        public event ConnectionDelegate OnTimeout;

        private Socket _socket;
        private System.IO.Stream _streamToSend;

        public IPEndPoint EndPoint { get; set; }
        public Params.Buffer ReceiveBuffer { get; set; }
        public Params.Buffer SendBuffer { get; set; }
        public bool IsConnected { get { return (_socket != null && _socket.Connected); } }

        public Connection(Params.Connection param)
        {
            EndPoint = param.EndPoint;
            ReceiveBuffer = param.ReceiveBuffer;
            SendBuffer = param.SendBuffer;
        }

        public void ConnectAsync()
        {
            SocketAsyncEventArgs socketArgs;

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

            try
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SendBuffer.Timeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveBuffer.Timeout);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, SendBuffer.Size);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ReceiveBuffer.Size);
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
                socketArgs = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = EndPoint
                };
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
                socketArgs.UserToken = new ConnectionAsyncEventArgs(new Timeout(SendBuffer.Timeout, Connect_Timeout));

                Logger.Network.Debug("Connecting to " + EndPoint.Address.ToString() + " on " + EndPoint.Port.ToString() + "...");

                try
                {
                    if (!_socket.ConnectAsync(socketArgs))
                    {
                        Logger.Network.Debug("ConnectAsync completed synchronously.");
                        Connect_Completed(null, socketArgs);
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

        private void Connect_Timeout()
        {
            Logger.Network.Error("Timeout during connection.");
            CloseSocketAndTimeout();
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            ConnectionAsyncEventArgs args = (ConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(e.UserToken))
                return;

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
            if (obj.GetType() != typeof(ConnectionAsyncEventArgs))
                throw new ArgumentException("Argument must be of type ConnectionAsyncEventArgs");

            return TryStopTimeout((ConnectionAsyncEventArgs)obj);
        }

        private bool TryStopTimeout(ConnectionAsyncEventArgs userToken)
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

        public void DisconnectAsync()
        {
            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();

            if (_socket != null)
            {
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Close_Completed);
                lock (_socket)
                {
                    socketArgs.UserToken = new ConnectionAsyncEventArgs(new Timeout(SendBuffer.Timeout, Close_Timeout));

                    Logger.Network.Debug("Disconnecting the socket and closing...");

                    try
                    {
                        if (!_socket.DisconnectAsync(socketArgs))
                        {
                            Logger.Network.Debug("DisconnectAsync completed synchronously.");
                            Close_Completed(null, socketArgs);
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
            if (!TryStopTimeout(e.UserToken))
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
            Logger.Network.Error("Timeout during closing.");
            CloseSocketAndTimeout();
        }

        public void SendAsync(byte[] buffer, int offset, int length)
        {
            if (!IsConnected)
                throw new TcpConnectionException("Socket is closed or not ready");
            if (length > SendBuffer.Size)
                throw new TcpConnectionException("Length must be less than or equal to the size of the send buffer.");

            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();

            socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(SendAsync_Completed);
            socketArgs.SetBuffer(buffer, offset, length);
            socketArgs.UserToken = new ConnectionAsyncEventArgs(new Timeout(SendBuffer.Timeout, SendAsync_Timeout));
            
            lock (_socket)
            {
                try
                {
                    if (!_socket.SendAsync(socketArgs))
                    {
                        Logger.Network.Debug("SendAsync completed synchronously.");
                        SendAsync_Completed(null, socketArgs);
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

        private void SendAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            ConnectionAsyncEventArgs userToken = (ConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;

            if (OnSendComplete != null) OnSendComplete(this);
        }

        private void SendAsync_Timeout()
        {
            Logger.Network.Error("Timeout during sending request.");
            CloseSocketAndTimeout();
        }

        public void ReceiveAsync()
        {
            SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
            socketArgs.SetBuffer(new byte[ReceiveBuffer.Size], 0, ReceiveBuffer.Size);
            socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveAsync_Completed);
            socketArgs.UserToken = new ConnectionAsyncEventArgs(new Timeout(ReceiveBuffer.Timeout, ReceiveAsync_Timeout));
            
            try
            {
                if (!_socket.ReceiveAsync(socketArgs))
                {
                    Logger.Network.Debug("ReceiveAsync completed synchronously.");
                    ReceiveAsync_Completed(null, socketArgs);
                }
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while receiving from the socket.", e);
                if (OnError != null) OnError(this, "Exception receiving from socket.", e);
                else throw;
            }
        }

        private void ReceiveAsync_Timeout()
        {
            Logger.Network.Error("Timeout during receiving from socket.");
            CloseSocketAndTimeout();
        }

        private void ReceiveAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            ConnectionAsyncEventArgs userToken = (ConnectionAsyncEventArgs)e.UserToken;

            if (!TryStopTimeout(userToken))
                return;

            if (OnReceiveComplete != null) OnReceiveComplete(this, e.Buffer, e.Offset, e.Count);
        }
    }
}
