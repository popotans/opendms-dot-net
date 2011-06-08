using System;
using System.Net;
using System.Net.Sockets;

namespace OpenDMS.Networking.Http
{
    public class HttpNetworkStream
    {
        private class Buffer
        {
            public byte[] BufferBytes { get; set; }
            public int Offset { get; set; }
            public int Length { get; set; }
            public Timeout Timeout { get; set; }
            public int BytesReadFromPrepend { get; set; }
        }

        private class BufferWithString : Buffer
        {
            public string String { get; set; }
        }

        private class BufferWithStream : Buffer
        {
            public System.IO.Stream Stream { get; set; }
        }

        public delegate void ProgressDelegate(HttpNetworkStream sender, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void TimeoutDelegate(HttpNetworkStream sender);
        public event TimeoutDelegate OnTimeout;
        public delegate void CompleteBufferOperationDelegate(HttpNetworkStream sender, DirectionType direction, byte[] buffer, int offset, int length);
        public event CompleteBufferOperationDelegate OnBufferOperationComplete;
        public delegate void CompleteStringOperationDelegate(HttpNetworkStream sender, string result);
        public event CompleteStringOperationDelegate OnStringOperationComplete;
        public delegate void CompleteStreamOperationDelegate(HttpNetworkStream sender, System.IO.Stream stream);
        public event CompleteStreamOperationDelegate OnStreamOperationComplete;
        public delegate void ErrorDelegate(HttpNetworkStream sender, string message, Exception exception);
        public event ErrorDelegate OnError;

        private PrependableNetworkStream _stream = null;
        private ulong _contentLength = 0;
        private Socket _socket = null;
        private ulong _bytesSent = 0;
        private ulong _bytesReceived = 0;
        private NetworkBuffer _prependBuffer = null;

        public bool CanRead { get { return _stream.CanRead; } }
        public bool CanSeek { get { return _stream.CanSeek; } }
        public bool CanTimeout { get { return _stream.CanTimeout; } }
        public bool CanWrite { get { return _stream.CanWrite; } }
        public bool DataAvailable { get { return _stream.DataAvailable; } }
        public long Length { get { return _stream.Length; } }
        public long Position { get { return _stream.Position; } set { _stream.Position = value; } }
        public int ReadTimeout { get { return _stream.ReadTimeout; } set { _stream.ReadTimeout = value; } }
        public Socket Socket { get { return _socket; } }
        public int WriteTimeout { get { return _stream.WriteTimeout; } set { _stream.WriteTimeout = value; } }

        public HttpNetworkStream(ulong contentLength, Socket socket, 
            System.IO.FileAccess fileAccess, bool ownsSocket)
        {
            _contentLength = contentLength;
            _socket = socket;
            _stream = new PrependableNetworkStream(socket, fileAccess, ownsSocket);
        }


        public HttpNetworkStream(ulong contentLength, byte[] prependToStream, Socket socket, System.IO.FileAccess fileAccess, bool ownsSocket)
            : this(contentLength, socket, fileAccess, ownsSocket)
        {
            _prependBuffer = new NetworkBuffer(prependToStream);
        }

        public void Close()
        {
            _stream.Close();
            _stream.Dispose();
        }

        private bool TryStartTimeout(int milliseconds, out Timeout timeout, Timeout.TimeoutEvent onTimeout)
        {
            timeout = null;

            try
            {
                timeout = new Timeout(_socket.ReceiveTimeout).Start();
                timeout.OnTimeout += onTimeout;
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

        private bool TryStopTimeout(Timeout timeout)
        {
            try
            {
                timeout.Stop();
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

        public void ReadAsync(byte[] buffer, int offset, int count)
        {
            StreamAsyncEventArgs args = new StreamAsyncEventArgs();
            Timeout timeout = null;
            int lengthMinusPrepend = count;

            if (!TryStartTimeout(_socket.ReceiveTimeout, out timeout, 
                new Timeout.TimeoutEvent(ReadAsync_OnTimeout)))
                return;

            
            // We should only try if the total bytes received is less than the content length
            // otherwise we fall into an infinite wait
            if (_contentLength > 0 &&
                _bytesReceived < _contentLength)
            {
                args.Complete = ReadAsync_Callback;
                args.UserToken = timeout;

                // Trim down the length to read to the content length if necessary
                if (_contentLength < (ulong)args.Count + _bytesReceived)
                    throw new ContentLengthExceededException("Content length exceeded.");

                args.SetBuffer(buffer, offset, count);
                 
                try
                {
                    _stream.ReadAsync(args);
                }
                catch (Exception e)
                {
                    Logger.Network.Error("An exception occurred while calling _stream.BeginRead.", e);
                    if (OnError != null) OnError(this, "Exception calling _stream.BeginRead", e);
                    else throw;
                }
            }
            else
                throw new ContentLengthExceededException("The read was requested starting outside the content length.");
        }

        private void ReadAsync_OnTimeout()
        {
            Logger.Network.Error("Timeout during read.");
            try
            {
                if (OnTimeout != null) OnTimeout(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadAsync_OnTimeout in the OnTimeout event.", ex);
                throw;
            }
        }

        private void ReadAsync_Callback(StreamAsyncEventArgs e)
        {
            _bytesReceived += (ulong)e.Count;

            if (!TryStopTimeout((Timeout)e.UserToken))
                return;

            try
            {
                if (OnProgress != null)
                    OnProgress(this, DirectionType.Download, e.Count);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadAsync_Callback in the OnProgress event.", ex);
            }

            try
            {
                if (OnBufferOperationComplete != null)
                    OnBufferOperationComplete(this, DirectionType.Download,
                        e.Buffer, e.Offset, e.Count);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadAsync_Callback in the OnBufferOperationComplete event.", ex);
            }

        }

        public int Read(byte[] buffer, int offset, int length)
        {
            int amount = 0;

            // Synchronous so let any exceptions bubble up for the higher level

            // We should only try if the total bytes received is less than the content length
            // otherwise we fall into an infinite wait
            if (_contentLength > 0 &&
                _bytesReceived < _contentLength)
            {
                // Trim down the length to read to the content length if necessary
                if (_contentLength < (ulong)length)
                    length = (int)_contentLength;

                amount = _stream.Read(buffer, offset, length);
                _bytesReceived += (ulong)amount;
                try
                {
                    if (OnProgress != null) OnProgress(this, DirectionType.Download, amount);
                }
                catch (Exception e)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.Read in the OnProgress event.", e);
                    throw;
                }
            }

            return amount;
        }

        public void WriteAsync(StreamAsyncEventArgs e)
        {
            Timeout timeout = null;

            if (!TryStartTimeout(_socket.SendTimeout, out timeout,
                new Timeout.TimeoutEvent(WriteAsync_OnTimeout)))
                return;

            if (_bytesSent >= _contentLength)
                throw new ContentLengthExceededException("The read was requested starting outside the content length.");

            e.Complete = WriteAsync_Callback;
            e.UserToken = timeout;

            try
            {
                _stream.WriteAsync(e);
            }
            catch (Exception ex)
            {
                Logger.Network.Error("An exception occurred while calling _stream.BeginWrite.", ex);
                if (OnError != null) OnError(this, "Exception calling _stream.BeginWrite", ex);
                else throw;
            }
        }

        private void WriteAsync_OnTimeout()
        {
            Logger.Network.Error("Timeout during write.");
            try
            {
                if (OnTimeout != null) OnTimeout(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.WriteAsync_OnTimeout in the OnTimeout event.", ex);
                throw;
            }
        }

        private void WriteAsync_Callback(StreamAsyncEventArgs e)
        {
            _bytesSent += (ulong)e.Count;

            if (!TryStopTimeout((Timeout)e.UserToken))
                return;

            try
            {
                if (OnProgress != null)
                    OnProgress(this, DirectionType.Upload, e.Count);

            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.WriteAsync_Callback in the OnProgress event.", ex);
            }

            try
            {
                if (OnBufferOperationComplete != null)
                    OnBufferOperationComplete(this, DirectionType.Upload,
                        e.Buffer, e.Offset, e.Count);

            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.WriteAsync_Callback in the OnBufferOperationComplete event.", ex);
            }
        }
        
        public void Write(byte[] buffer, int offset, int length)
        {
            // Synchronous so let any exceptions bubble up for the higher level

            _stream.Write(buffer, offset, length);
            _bytesSent += (ulong)length;
            try
            {
                if (OnProgress != null) OnProgress(this, DirectionType.Upload, length);
            }
            catch (Exception e)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.Write in the OnProgress event.", e);
                throw;
            }
        }

        public void ReadToEndAsync()
        {
            Timeout timeout = null;
            StreamAsyncEventArgs args = new StreamAsyncEventArgs();

            args.Complete = ReadToEndAsync_Callback;
            args.UserToken = new Tuple<Timeout, string>(timeout, "");
            args.SetBuffer(new byte[_socket.ReceiveBufferSize], 0, _socket.ReceiveBufferSize);

            try
            {
                _stream.ReadAsync(args);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling ReadToEndAsync.", e);
                if (OnError != null) OnError(this, "Exception calling ReadToEndAsync", e);
                else throw;
            }
        }

        private void ReadToEndAsync_OnTimeout()
        {
            Logger.Network.Error("Timeout during ReadToEnd.");
            try
            {
                if (OnTimeout != null) OnTimeout(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadToEndAsync_OnTimeout in the OnTimeout event.", ex);
                throw;
            }
        }

        private void ReadToEndAsync_Callback(StreamAsyncEventArgs e)
        {
            Timeout timeout = null;
            Tuple<Timeout, string> userToken = (Tuple<Timeout, string>)e.UserToken;
            string str = userToken.Item2;

            _bytesReceived += (ulong)e.Count;

            if (!TryStopTimeout(userToken.Item1))
                return;

            try
            {
                str += System.Text.Encoding.ASCII.GetString(e.Buffer, e.Offset, e.Count);
            }
            catch (Exception ex)
            {
                Logger.Network.Error("An exception occurred while getting a string from the buffer.", ex);
                if (OnError != null)
                {
                    OnError(this, "Exception while getting a string from the buffer.", ex);
                    return;
                }
            }

            // Progress Event
            try
            {
                if (OnProgress != null)
                    OnProgress(this, DirectionType.Download, e.Count);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadToEndAsync_Callback in the OnProgress event.", ex);
            }
            
            // End?
            if (_bytesReceived > _contentLength)
                throw new ContentLengthExceededException("Content received was longer than the Content Length specified.");
            else if (_bytesReceived == _contentLength)
            {
                try
                {
                    if (OnStringOperationComplete != null)
                        OnStringOperationComplete(this, str);
                }
                catch (Exception ex)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.ReadToEndAsync_Callback in the OnBufferOperationComplete event.", ex);
                }
            }
            else
            {
                // content left to stream
                if (!TryStartTimeout(_socket.ReceiveTimeout, out timeout,
                    new Timeout.TimeoutEvent(ReadToEndAsync_OnTimeout)))
                    return;

                e.UserToken = new Tuple<Timeout, string>(timeout, str);
                e.SetBuffer(e.Buffer, 0, _socket.ReceiveBufferSize);

                try
                {
                    _stream.ReadAsync(e);
                }
                catch (Exception ex)
                {
                    Logger.Network.Error("An exception occurred while calling ReadToEndAsync.", ex);
                    if (OnError != null) OnError(this, "Exception calling ReadToEndAsync", ex);
                    else throw;
                }
            }
        }

        public string ReadToEnd()
        {
            // Synchronous so let any exceptions bubble up for the higher level

            byte[] buffer = new byte[_socket.ReceiveBufferSize];
            int bytesRead = 0;
            ulong totalBytesRead = 0;
            string str = "";

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += (ulong)bytesRead;
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (totalBytesRead > _contentLength)
                    throw new System.IO.InternalBufferOverflowException("The content extended beyond the specified content-length header.");
                else if (totalBytesRead == _contentLength)
                    break;
            }

            return str;
        }

        public void CopyToAsync(System.IO.Stream stream)
        {
            Timeout timeout = null;
            byte[] buffer = new byte[_socket.ReceiveBufferSize];

            if (!TryStartTimeout(_socket.ReceiveTimeout, out timeout,
                new Timeout.TimeoutEvent(CopyToAsync_OnTimeout)))
                return;
            
            try
            {
                _stream.BeginRead(buffer, 0, buffer.Length,
                    new AsyncCallback(CopyToAsync_Callback),
                    new BufferWithStream()
                    {
                        BufferBytes = buffer,
                        Offset = 0,
                        Length = buffer.Length,
                        Stream = stream,
                        Timeout = timeout
                    });
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling CopyToAsync.", e);
                if (OnError != null) OnError(this, "Exception calling CopyToAsync", e);
                else throw;
            }
        }

        private void CopyToAsync_OnTimeout()
        {
            Logger.Network.Error("Timeout during CopyTo.");
            try
            {
                if (OnTimeout != null) OnTimeout(this);
            }
            catch (Exception ex)
            {
                // Ignore it, its the higher level's job to deal with it.
                Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.CopyToAsync_OnTimeout in the OnTimeout event.", ex);
                throw;
            }
        }

        private void CopyToAsync_Callback(IAsyncResult result)
        {
            Timeout timeout = null;
            BufferWithStream buffer = null;
            int bytesRead = 0;

            try
            {
                buffer = (BufferWithStream)result.AsyncState;

                if (!TryStopTimeout(buffer.Timeout))
                    return;

                bytesRead = _stream.EndRead(result);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while calling _stream.EndRead.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception calling _stream.EndRead", e);
                    return;
                }
                else throw;
            }

            _bytesReceived += (ulong)bytesRead;

            try
            {
                buffer.Stream.Write(buffer.BufferBytes, buffer.Offset, buffer.Length);
            }
            catch (Exception e)
            {
                Logger.Network.Error("An exception occurred while writing a block from the IO.Stream to this HttpNetworkStream.", e);
                if (OnError != null)
                {
                    OnError(this, "Exception while writing a block from the IO.Stream to this NetworkStream.", e);
                    return;
                }
            }

            if (bytesRead > 0)
            {
                try
                {
                    if (OnProgress != null)
                        OnProgress(this, DirectionType.Download, bytesRead);
                }
                catch (Exception e)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.CopyToAsync_Callback in the OnProgress event.", e);
                }

                if (!TryStartTimeout(_socket.ReceiveTimeout, out timeout,
                    new Timeout.TimeoutEvent(CopyToAsync_OnTimeout)))
                    return;

                try
                {
                    _stream.BeginRead(buffer.BufferBytes, 0, buffer.Length,
                        new AsyncCallback(CopyToAsync_Callback),
                        new BufferWithStream()
                        {
                            BufferBytes = buffer.BufferBytes,
                            Offset = 0,
                            Length = buffer.Length,
                            Stream = buffer.Stream,
                            Timeout = timeout
                        });
                }
                catch (Exception e)
                {
                    Logger.Network.Error("An exception occurred while calling _stream.BeginRead.", e);
                    if (OnError != null) OnError(this, "Exception calling _stream.BeginRead", e);
                    else throw;
                }
            }
            else
            {
                try
                {
                    if (OnStreamOperationComplete != null)
                        OnStreamOperationComplete(this, buffer.Stream);
                }
                catch (Exception e)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.CopyToAsync_Callback in the OnBufferOperationComplete event.", e);
                }
            }
        }

        public void CopyTo(System.IO.Stream stream)
        {
            // Synchronous so let any exceptions bubble up for the higher level

            byte[] buffer = new byte[_socket.ReceiveBufferSize];
            int bytesRead = 0;

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
            {
                stream.Write(buffer, 0, bytesRead);

                try
                {
                    if (OnProgress != null) OnProgress(this, DirectionType.Download, bytesRead);
                }
                catch (Exception e)
                {
                    // Ignore it, its the higher level's job to deal with it.
                    Logger.Network.Error("An unhandled exception was caught by HttpNetworkStream.CopyTo in the OnProgress event.", e);
                    throw;
                }
            }
        }
    }
}
