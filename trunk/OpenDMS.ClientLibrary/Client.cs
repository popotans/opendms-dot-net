using System;
using Api = OpenDMS.Networking.Api;
using Tcp = OpenDMS.Networking.Protocols.Tcp;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.ClientLibrary
{
    public class Client
    {
        public delegate void ConnectionDelegate(Client sender, Http.HttpConnection connection, TimeSpan duration);
        public delegate void ErrorDelegate(Client sender, Http.HttpConnection connection, string message, Exception exception, TimeSpan duration);
        public delegate void ProgressDelegate(Client sender, Http.HttpConnection connection, Tcp.DirectionType direction, int packetSize, TimeSpan duration, decimal requestPercentSent, decimal responsePercentReceived);
        public delegate void CompletionDelegate(Client sender, Http.HttpConnection connection, Http.Response httpResponse, Api.Responses.ResponseBase apiResponse, TimeSpan duration);

        private Http.Client _client;

        public Client()
        {
            _client = new Http.Client();
        }

        public void Execute(Api.Requests.RequestBase apiRequest, ClientArgs args)
        {
            Http.Request httpRequest;
            DateTime start = DateTime.Now;
            Tcp.Params.Buffer receiveBufferSettings, sendBufferSettings;
            Http.HttpConnection.ConnectionDelegate onDisconnect = null, onTimeout = null, onConnect = null;
            Http.HttpConnection.CompletionDelegate onComplete = null;
            Http.HttpConnection.ErrorDelegate onError = null;
            Http.HttpConnection.ProgressDelegate onProgress = null;

            httpRequest = apiRequest.CreateRequest(args.Uri, args.ContentType);

            receiveBufferSettings = new Tcp.Params.Buffer() { Size = args.ReceiveBufferSize, Timeout = args.ReceiveTimeout };
            sendBufferSettings = new Tcp.Params.Buffer() { Size = args.SendBufferSize, Timeout = args.SendTimeout };
            
            onConnect += delegate(Http.HttpConnection sender)
            {
                if (args.OnConnect != null) args.OnConnect(this, sender, DateTime.Now - start);
            };
            onDisconnect += delegate(Http.HttpConnection sender)
            {
                if (args.OnDisconnect != null) args.OnDisconnect(this, sender, DateTime.Now - start);
            };
            onTimeout += delegate(Http.HttpConnection sender)
            {
                if (args.OnTimeout != null) args.OnTimeout(this, sender, DateTime.Now - start);
            };
            onComplete += delegate(Http.HttpConnection sender, Http.Response response)
            {
                Api.Responses.ResponseBase apiResponse = null;

                if (apiRequest.Type == typeof(Api.Requests.Ping))
                {
                    apiResponse = Api.Responses.Pong.BuildFrom(apiRequest);
                }
                else if (apiRequest.Type == typeof(Api.Requests.Authentication))
                {
                    throw new NotImplementedException();
                }

                if (args.OnComplete != null) args.OnComplete(this, sender, response, apiResponse, DateTime.Now - start);
            };
            onError += delegate(Http.HttpConnection sender, string message, Exception exception)
            {
                if (args.OnError != null) args.OnError(this, sender, message, exception, DateTime.Now - start);
            };
            onProgress += delegate(Http.HttpConnection sender, Tcp.DirectionType direction, int packetSize, decimal requestPercentSent, decimal responsePercentReceived)
            {
                if (args.OnProgress != null) args.OnProgress(this, sender, direction, packetSize, DateTime.Now - start, requestPercentSent, responsePercentReceived);
            };

            _client = new Http.Client();

            start = DateTime.Now;
            _client.Execute(httpRequest, receiveBufferSettings, sendBufferSettings, onConnect, onDisconnect, onError, onProgress, onTimeout, onComplete);
        }
    }
}
