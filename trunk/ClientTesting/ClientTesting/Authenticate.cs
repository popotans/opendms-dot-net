using System;
using Http = OpenDMS.Networking.Http;
using Api = OpenDMS.Networking.Api;

namespace ClientTesting
{
    public class Authenticate : TestBase
    {
        public static string Username = "Lucas";
        public static string Password = "asdasdasdasdasdasd";
        private Api.Requests.Authentication _auth;
        private DateTime _start;

        public Authenticate(FrmMain window)
            : base(window)
        {
        }

        public override void Test()
        {
            OpenDMS.ClientLibrary.Client client;
            Api.Requests.Authentication auth;

            Clear();

            WriteLine("Starting Authentication test...");

            auth = new Api.Requests.Authentication();
            auth.Username = Username;
            auth.ClearTextPassword = Password;

            client = new OpenDMS.ClientLibrary.Client(auth);
            _start = DateTime.Now;
            client.Execute(new OpenDMS.ClientLibrary.ClientArgs()
            {
                Uri = new Uri("http://" + FrmMain.HOST + ":" + FrmMain.PORT.ToString() + "/_auth"),
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
            int bytesRead = 0;
            byte[] buffer = new byte[8192];
            System.IO.FileStream fs = new System.IO.FileStream(@"C:\OpenDMS_Client\Response.txt", System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);

            sw.WriteLine("Response Code: " + response.ResponseCode.ToString());
            for (int i = 0; i < response.Headers.Count; i++)
                sw.Write(response.Headers.AllKeys[i] + ": " + response.Headers[i].ToString() + "\r\n");

            sw.Flush();

            if (response.Stream != null)
            {
                while ((bytesRead = response.Stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    bw.Write(buffer, 0, bytesRead);
                }
            }

            bw.Flush();

            sw.Close();
            bw.Close();
            sw.Dispose();
            bw.Dispose();


            //Api.Responses.Authentication resp = Api.Responses.Response<Api.Responses.Authentication>.Parse(response);

            //WriteLine("Server execution time: " + resp.Duration.Milliseconds.ToString() + "ms.");
            //WriteLine("Round-trip completed in " + (DateTime.Now - _start).Milliseconds.ToString() + "ms.");
            //WriteLine("JSON response:\r\n" + resp.FullContent.ToString());
        }

        void OnClose(Http.Client sender, Http.Connection connection)
        {
            WriteLine("Connection closed.");
        }
    }
}
