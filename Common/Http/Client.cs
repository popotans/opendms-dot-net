using System;

namespace Common.Http
{
    public class Client
    {
        public delegate void DataSentDelegate(Client sender, Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        public delegate void DataReceivedDelegate(Client sender, Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        public event DataSentDelegate OnDataSent;
        public event DataReceivedDelegate OnDataReceived;

        public Methods.HttpResponse Execute(Methods.HttpRequest request, System.IO.Stream stream)
        {
            Network.HttpConnectionFactory connFactory = null;
            Network.HttpConnection connection = null;

            connFactory = new Network.HttpConnectionFactory();
            connection = connFactory.GetConnection(request.Uri);

            return Execute(request, connection, stream);
        }

        public Methods.HttpResponse Execute(Methods.HttpRequest request, System.IO.Stream stream,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            Network.HttpConnectionFactory connFactory = null;
            Network.HttpConnection connection = null;

            connFactory = new Network.HttpConnectionFactory();
            connection = connFactory.GetConnection(request.Uri, sendTimeout, receiveTimeout,
                sendBufferSize, receiveBufferSize);

            return Execute(request, connection, stream);
        }

        private Methods.HttpResponse Execute(Methods.HttpRequest request, Network.HttpConnection connection, 
            System.IO.Stream stream)
        {
            Methods.HttpResponse response = null;

            // Subscribe to events
            connection.OnDataReceived += new Network.HttpConnection.DataReceivedDelegate(Connection_OnDataReceived);
            connection.OnDataSent += new Network.HttpConnection.DataSentDelegate(Connection_OnDataSent);

            if (stream != null)
            {
                if (stream.CanSeek) stream.Position = 0;
                request.ContentLength = stream.Length.ToString();
                connection.SendRequestHeaderAndStream(request, stream);
            }
            else
            {
                request.ContentLength = "0";
                connection.SendRequestHeaderOnly(request);
            }

            response = connection.ReceiveResponseHeaders();

            if (Utilities.GetContentLength(response.Headers) > 0)
                connection.ReceiveResponseBody(response);

            // If 100 then we are about to get another body
            if (response.ResponseCode == 100)
            {
                connection.ReceiveResponseBody(response);
            }

            return response;
        }

        void Connection_OnDataSent(Network.HttpConnection sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnDataSent != null)
                OnDataSent(this, sender, packetSize, headersTotal, contentTotal, total);
        }

        void Connection_OnDataReceived(Network.HttpConnection sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnDataReceived != null)
                OnDataReceived(this, sender, packetSize, headersTotal, contentTotal, total);
        }
    }
}
