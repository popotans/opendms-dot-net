using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Client
    {
        //private ulong _bytesReceivedContentOnly = 0;
        //private ulong _bytesReceivedHeadersOnly = 0;
        //private ulong _bytesReceivedTotal = 0;
        //private ulong _bytesSentContentOnly = 0;
        //private ulong _bytesSentHeadersOnly = 0;
        //private ulong _bytesSentTotal = 0;

        //private ulong _contentLengthRx = 0;
        //private ulong _contentLengthTx = 0;
        //private ulong _headersLengthRx = 0;
        //private ulong _headersLengthTx = 0;

        //private bool _isBusy = false;

        public Client()
        {
        }

        public void Execute(Request request,
            Tcp.Params.Buffer receiveBufferSettings, Tcp.Params.Buffer sendBufferSettings)
        {
            Execute(request, receiveBufferSettings, sendBufferSettings);
        }

        public void Execute(Request request, System.IO.Stream stream, 
            Tcp.Params.Buffer receiveBufferSettings, Tcp.Params.Buffer sendBufferSettings,
            HttpConnection.ConnectionDelegate onConnectCallback,
            HttpConnection.ConnectionDelegate onDisconnectCallback,
            HttpConnection.ErrorDelegate onErrorCallback,
            HttpConnection.ProgressDelegate onProgressCallback,
            HttpConnection.ConnectionDelegate onTimeoutCallback,
            HttpConnection.CompletionDelegate onCompleteCallback)
        {
            HttpConnection conn;

            conn = new HttpConnection(request.RequestLine.RequestUri, receiveBufferSettings, sendBufferSettings);

            if (onConnectCallback != null) conn.OnConnect += onConnectCallback;
            if (onDisconnectCallback != null) conn.OnDisconnect += onDisconnectCallback;
            if (onErrorCallback != null) conn.OnError += onErrorCallback;
            if (onProgressCallback != null) conn.OnProgress += onProgressCallback;
            if (onTimeoutCallback != null) conn.OnTimeout += onTimeoutCallback;
            if (onCompleteCallback != null) conn.OnComplete += onCompleteCallback;

            conn.SendRequestAsync(request);
        }
    }
}
