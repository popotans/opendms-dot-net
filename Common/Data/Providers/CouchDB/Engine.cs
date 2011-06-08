using System;
using Common.Http;

namespace Common.Data.Providers.CouchDB
{
    public class Engine
    {
        public delegate void TimeoutDelegate(Engine sender, Commands.Base command, Client client, Http.Network.HttpConnection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Engine sender, Commands.Base sender, Client client, Http.Network.HttpConnection connection, Http.Network.DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Engine sender, Commands.Base sender, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDlegate(Engine sender, Commands.Base sender, Client client, Http.Network.HttpConnection connection, Http.Methods.HttpResponse response);
        public event CompletionDlegate OnComplete;

        public Engine()
        {
        }

        public void ExecuteCommand(Commands.Base cmd)
        {
            cmd.Execute();
        }
    }
}
