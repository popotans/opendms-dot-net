using System;
using System.Net.Sockets;

namespace Common.Http.Network
{
    public class HttpNetworkStream
    {
        public delegate void DataSentDelegate(ulong amount, ulong total);
        public delegate void DataReceivedDelegate(ulong amount, ulong total);
        public event DataSentDelegate OnDataSent;
        public event DataReceivedDelegate OnDataReceived;

        private Methods.HttpResponse _response = null;
        private NetworkStream _stream = null;
        private Socket _socket = null;
        private ulong _bytesSent = 0;
        private ulong _bytesReceived = 0;

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

        public HttpNetworkStream(Methods.HttpResponse response, Socket socket, 
            System.IO.FileAccess fileAccess, bool ownsSocket)
        {
            _response = response;
            _socket = socket;
            _stream = new NetworkStream(socket, fileAccess, ownsSocket);
        }

        public void Close()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            int amount = 0;

            if (Utilities.GetContentLength(_response.Headers) > 0 &&
                _bytesReceived < Utilities.GetContentLength(_response.Headers))
            {
                amount = _stream.Read(buffer, offset, length);
                _bytesReceived += (ulong)amount;
                if (OnDataReceived != null) OnDataReceived((ulong)amount, _bytesReceived);
            }

            return amount;
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            _stream.Write(buffer, offset, length);
            _bytesSent += (ulong)length;
            if (OnDataSent != null) OnDataSent((ulong)length, _bytesSent);
        }

        public string ReadToEnd()
        {
            byte[] buffer = new byte[_socket.ReceiveBufferSize];
            int bytesRead = 0;
            ulong totalBytesRead = 0;
            string str = "";

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += (ulong)bytesRead;
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (totalBytesRead > Utilities.GetContentLength(_response.Headers))
                    throw new System.IO.InternalBufferOverflowException("The content extended beyond the specified content-length header.");
                else if (totalBytesRead == Utilities.GetContentLength(_response.Headers))
                    break;
            }

            return str;
        }

        public void CopyTo(System.IO.Stream stream)
        {
            byte[] buffer = new byte[_socket.ReceiveBufferSize];
            int bytesRead = 0;

            while ((bytesRead = Read(buffer, 0, buffer.Length)) > 0)
                stream.Write(buffer, 0, bytesRead);

            _stream.Flush();
        }
    }
}
