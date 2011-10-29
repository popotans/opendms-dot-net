using System;
using Http = OpenDMS.Networking.Http;
using Api = OpenDMS.Networking.Api;

namespace ClientTesting
{
    public class Ping : TestBase
    {
        private DateTime _start;

        public Ping(FrmMain window)
            : base(window)
        {
        }

        public override void Test()
        {
            OpenDMS.ClientLibrary.Client client;
            Api.Requests.Ping ping;
            
            Clear();

            WriteLine("Starting Ping test...");

            ping = new Api.Requests.Ping();

            client = new OpenDMS.ClientLibrary.Client(ping);
            _start = DateTime.Now;
            client.Execute(new OpenDMS.ClientLibrary.ClientArgs()
            {
                Uri = new Uri("http://" + FrmMain.HOST + ":" + FrmMain.PORT.ToString() + "/_ping"),
                ContentType = "application/json",
                OnClose = OnClose,
                OnComplete = OnComplete,
                OnError = OnError,
                OnProgress = OnProgress,
                OnTimeout = OnTimeout,
                SendTimeout = FrmMain.SendTimeout,
                ReceiveTimeout = FrmMain.ReceiveTimeout,
                SendBufferSize = FrmMain.SendBufferSize,
                ReceiveBufferSize = FrmMain.ReceiveBufferSize, 
            });
        }

        void OnTimeout(Http.Client sender, Http.Connection connection)
        {
            WriteLine("Timeout occurred.");
        }

        void OnProgress(Http.Client sender, Http.Connection connection, Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            WriteLine("Progress - Sent: " + sendPercentComplete.ToString() + " Received: " + receivePercentComplete.ToString());
        }

        void OnError(Http.Client sender, string message, Exception exception)
        {
            WriteLine("Error - Message: " + message);
        }

        void OnComplete(Http.Client sender, Http.Connection connection, Http.Methods.Response response)
        {
            Api.Responses.Pong resp = Api.Responses.Response<Api.Responses.Pong>.Parse(response);

            WriteLine("Server execution time: " + resp.Duration.Milliseconds.ToString() + "ms.");
            WriteLine("Round-trip completed in " + (DateTime.Now - _start).Milliseconds.ToString() + "ms.");
            WriteLine("JSON response:\r\n" + resp.FullContent.ToString());
        }

        void OnClose(Http.Client sender, Http.Connection connection)
        {
            WriteLine("Connection closed.");
        }
    }
}
