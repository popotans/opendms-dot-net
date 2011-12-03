using System;
using Tcp = OpenDMS.Networking.Protocols.Tcp;
using Http = OpenDMS.Networking.Protocols.Http;
using Api = OpenDMS.Networking.Api;
using ClientLib = OpenDMS.ClientLibrary;

namespace ClientTesting
{
    public class Ping : TestBase
    {
        private DateTime _start;

        public Ping(FrmMain window, ClientLib.Client client)
            : base(window, client)
        {
        }

        public override void Test()
        {
            Api.Requests.Ping ping;
            
            Clear();

            WriteLine("Starting Ping test...");

            ping = new Api.Requests.Ping();

            _client = new ClientLib.Client();

            _start = DateTime.Now;
            _client.Execute(new Api.Requests.Ping(),
                new ClientLib.ClientArgs()
                {
                    Uri = new Uri("http://" + FrmMain.HOST + ":" + FrmMain.PORT.ToString() + "/_ping"),
                    ContentType = "application/json",
                    OnDisconnect = OnDisconnect,
                    OnComplete = OnComplete,
                    OnError = OnError,
                    OnProgress = OnProgress,
                    OnTimeout = OnTimeout,
                    SendTimeout = FrmMain.SendTimeout,
                    ReceiveTimeout = FrmMain.ReceiveTimeout,
                    SendBufferSize = FrmMain.SendBufferSize,
                    ReceiveBufferSize = FrmMain.ReceiveBufferSize,
                }
            );
        }

        void OnTimeout(ClientLib.Client sender, Http.HttpConnection connection, TimeSpan duration)
        {
            WriteLine("Timeout occurred.");
        }

        void OnProgress(ClientLib.Client sender, Http.HttpConnection connection, Tcp.DirectionType direction, int packetSize, TimeSpan duration, decimal requestPercentSent, decimal responsePercentComplete)
        {
            WriteLine("Progress - Sent: " + requestPercentSent.ToString() + " Received: " + responsePercentComplete.ToString());
        }

        void OnError(ClientLib.Client sender, Http.HttpConnection connection, string message, Exception exception, TimeSpan duration)
        {
            WriteLine("Error - Message: " + message);
        }

        void OnComplete(ClientLib.Client sender, Http.HttpConnection connection, Http.Response httpResponse, Api.Responses.ResponseBase apiResponse, TimeSpan duration)
        {
            Api.Responses.Pong resp = (Api.Responses.Pong)apiResponse;

            WriteLine("Server execution time: " + resp.Duration.Milliseconds.ToString() + "ms.");
            WriteLine("Round-trip completed in " + (DateTime.Now - _start).Milliseconds.ToString() + "ms.");
            WriteLine("JSON response:\r\n" + resp.FullContent.ToString());
        }

        void OnDisconnect(ClientLib.Client sender, Http.HttpConnection connection, TimeSpan duration)
        {
            WriteLine("Connection closed.");
        }
    }
}
