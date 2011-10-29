using System;
using Api = OpenDMS.Networking.Api;
using Http = OpenDMS.Networking.Http;

namespace OpenDMS.ClientLibrary
{
    public class Client
    {
        private DateTime _start;
        private Api.Requests.RequestBase _request;

        public TimeSpan Duration { get; private set; }

        public Client(Api.Requests.RequestBase request)
        {
            _request = request;
        }

        public void Execute(ClientArgs args)
        {
            long contentLength;
            Http.Methods.Request method;
            Http.Client client;
            Api.MultisourcedStream stream;
            
            stream = _request.MakeStream(out contentLength);
            method = _request.CreateRequest(args.Uri, args.ContentType, contentLength);

            client = new Http.Client();
            client.OnClose += delegate(Http.Client sender, Http.Connection connection)
            {
                Duration = DateTime.Now - _start;
            };
            client.OnComplete += delegate(Http.Client sender, Http.Connection connection, Http.Methods.Response response)
            {
                Duration = DateTime.Now - _start;
            };
            client.OnError += delegate(Http.Client sender, string message, Exception exception)
            {
                Duration = DateTime.Now - _start;
            };
            client.OnProgress += delegate(Http.Client sender, Http.Connection connection, Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                Duration = DateTime.Now - _start;
            };
            client.OnTimeout += delegate(Http.Client sender, Http.Connection connection)
            {
                Duration = DateTime.Now - _start;
            };
            client.OnClose += args.OnClose;
            client.OnComplete += args.OnComplete;
            client.OnError += args.OnError;
            client.OnProgress += args.OnProgress;
            client.OnTimeout += args.OnTimeout;

            _start = DateTime.Now;
            client.Execute(method, stream, args.SendTimeout, args.ReceiveTimeout,
                args.SendBufferSize, args.ReceiveBufferSize);
        }
    }
}
