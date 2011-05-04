using System;
using System.Net;
using System.Net.Sockets;

namespace Common.Http.Network
{
    public class HttpConnection
    {
        public delegate void DataSentDelegate(HttpConnection sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        public delegate void DataReceivedDelegate(HttpConnection sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        public event DataSentDelegate OnDataSent;
        public event DataReceivedDelegate OnDataReceived;

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

        public void Connect()
        {
            IPAddress ipaddress;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!IPAddress.TryParse(_uri.Host, out ipaddress))
                ipaddress = Dns.GetHostEntry(_uri.Host).AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipaddress, _uri.Port);

            _socket.Connect(remoteEP);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _sendTimeout);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _receiveTimeout);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _sendBufferSize);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _receiveBufferSize);
        }

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket.Dispose();
            }
        }

        public void SendRequestHeaderOnly(Methods.HttpRequest request)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready");

            int bytesSent = 0;

            Logger.Network.Debug("Sending request headers...");
            bytesSent = _socket.Send(System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request)));

            _bytesSentHeadersOnly += (ulong)bytesSent;
            _bytesSentTotal += (ulong)bytesSent;
            if (OnDataSent != null) OnDataSent(this, bytesSent, _bytesSentHeadersOnly, _bytesSentContentOnly, _bytesSentTotal);

            // Wait for response
            WaitForDataToArriveAtSocket(_receiveTimeout);

            if (_socket.Available <= 0)
            {
                Logger.Network.Debug("A timeout occurred while waiting on the server's response to the request headers.");
                throw new HttpNetworkTimeoutException("Timeout waiting on response.");
            }

            Logger.Network.Debug("Request headers have been sent and received by the server.");
        }


        public void SendRequestHeaderAndStream(Methods.HttpRequest request, System.IO.Stream stream)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready");
            
            byte[] buffer = new byte[_sendBufferSize];
            int bytesRead = 0;
            int bytesSent = 0;

            if (stream != null)
            {
                Logger.Network.Debug("Found data to send, configuring use of 100-Continue.");
                request.Headers.Add("Expect", "100-Continue");
            }

            byte[] header = System.Text.Encoding.ASCII.GetBytes(GetRequestHeader(request).ToString());

            // Send headers
            Logger.Network.Debug("Sending request headers...");
            bytesSent = Send(header, 0, header.Length, _sendTimeout);

            _bytesSentHeadersOnly += (ulong)bytesSent;
            _bytesSentTotal += (ulong)bytesSent;
            if (OnDataSent != null) OnDataSent(this, bytesSent, _bytesSentHeadersOnly, _bytesSentContentOnly, _bytesSentTotal);

            // Do we need to wait for a 100-Continue response?
            if (!string.IsNullOrEmpty(request.Headers.Get("Expect")) &&
                request.Headers.Get("Expect") == "100-Continue")
            {
                Logger.Network.Debug("Waiting on the 100-Continue message...");
                WaitForDataToArriveAtSocket(_receiveTimeout);
                if (_socket.Available > 0)
                {
                    // Read the 100-Continue response
                    Methods.HttpResponse response = ReceiveResponseHeaders();
                    if (response.ResponseCode != 100)
                    {
                        Logger.Network.Error("100-Continue server response was not received.");
                        throw new HttpNetworkException("Reponse returned before data was sent, but it is not 100-continue.");
                    }
                }
                else
                {
                    Logger.Network.Error("The expected 100-Continue response was not received from the server within the " + _receiveTimeout.ToString() + "ms timeout period.");
                    throw new HttpNetworkTimeoutException("A timeout occurred while waiting on the 100-Continue response.");
                }
            }

            Logger.Network.Debug("100-Continue server response was received, preparing to send data...");

            // Send payload
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                bytesSent = Send(buffer, 0, bytesRead, _sendTimeout);
                _bytesSentContentOnly += (ulong)bytesSent;
                _bytesSentTotal += (ulong)bytesSent;
            }
        }

        private void WaitForDataToArriveAtSocket(int timeout)
        {
            int counter = 0;
            // wait another timeout period for the response to arrive.
            while (!(_socket.Available > 0) && counter < (timeout / 100))
            {
                counter++;
                System.Threading.Thread.Sleep(100);
            }
        }

        private int Send(byte[] buffer, int offset, int length, int timeout)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;

            while (sent < length)
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new HttpNetworkTimeoutException("Send timed out");

                try
                {
                    sent += _socket.Send(buffer, offset + sent, length - sent, SocketFlags.None);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.WouldBlock ||
                        e.SocketErrorCode == SocketError.IOPending ||
                        e.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // Buffer might be full, lets give it a few milliseconds and try again
                        // This will only keep retrying until the timeout passes.
                        System.Threading.Thread.Sleep(30);
                    }
                    else if (e.SocketErrorCode == SocketError.TimedOut)
                        throw new HttpNetworkTimeoutException(e.Message);
                }
            }

            return sent;
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

        public Methods.HttpResponse ReceiveResponseHeaders()
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready.");

            Methods.HttpResponse response = new Methods.HttpResponse();
            string header = "";
            byte[] bytes;
            System.Text.RegularExpressions.MatchCollection matches;

            Logger.Network.Debug("Starting receiving of headers...");

            WaitForDataToArriveAtSocket(_receiveTimeout);

            // We must receive byte by byte to prevent reading any of the data stream
            // Perhaps this should be optimized to read chunks until the headers are done storing
            // any remainder in a temporary buffer which is first read once the ReceiveResponseBody is called
            bytes = new byte[1];
            while (_socket.Receive(bytes, 0, 1, SocketFlags.None) > 0)
            {
                header += System.Text.Encoding.ASCII.GetString(bytes, 0, 1);
                _bytesReceivedHeadersOnly++;
                _bytesReceivedTotal++;
                if (bytes[0] == '\n' && header.EndsWith("\r\n\r\n"))
                    break;
            }
            
            matches = new System.Text.RegularExpressions.Regex("[^\r\n]+").Matches(header.TrimEnd('\r', '\n'));
            for (int n = 1; n < matches.Count; n++)
            {
                string[] strItem = matches[n].Value.Split(new char[] { ':' }, 2);
                if (strItem.Length > 1)
                {
                    if (!strItem[0].Trim().ToLower().Equals("set-cookie"))
                        response.Headers.Add(strItem[0].Trim(), strItem[1].Trim());
                    else
                        response.Headers.Add(strItem[0].Trim(), strItem[1].Trim());
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
                catch (Exception e)
                {
                    throw new HttpNetworkException("Response Code is missing from the response");
                }
            }
            
            Logger.Network.Debug("Receiving of headers is complete.");

            if (OnDataReceived != null) OnDataReceived(this, (int)_bytesReceivedHeadersOnly,
                _bytesReceivedHeadersOnly, _bytesReceivedContentOnly, _bytesReceivedTotal);

            return response;
        }

        public void ReceiveResponseBody(Methods.HttpResponse response)
        {
            if (!IsConnected)
                throw new HttpNetworkException("Socket is closed or not ready.");

            Logger.Network.Debug("Allocating a network stream to the response content...");

            string chunkedHeader = Utilities.GetTransferEncoding(response.Headers);

            if (chunkedHeader != null &&
                chunkedHeader.ToLower() == "chunked")
            {
                throw new HttpNetworkException("Receiving of chunked data is not supported.");
            }
            else
            {
                if (Utilities.GetContentLength(response.Headers) > 0)
                {
                    response.Stream = new HttpNetworkStream(response, _socket, System.IO.FileAccess.Read, false);
                    response.Stream.OnDataReceived += new HttpNetworkStream.DataReceivedDelegate(Stream_OnDataReceived);
                    Logger.Network.Debug("A network stream has been successfully attached to the response content.");
                }
                else
                {
                    Logger.Network.Debug("There was no response content on which to attach a network stream.");
                }
            }

            return;
        }

        void Stream_OnDataReceived(ulong amount, ulong total)
        {
            _bytesReceivedTotal += amount;
            _bytesReceivedContentOnly += amount;
            if (OnDataReceived != null) OnDataReceived(this, (int)amount, _bytesReceivedHeadersOnly, 
                _bytesReceivedContentOnly, _bytesReceivedTotal);
        }
    }
}
