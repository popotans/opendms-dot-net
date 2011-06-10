using System;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public class Engine
    {
        public delegate void TimeoutDelegate(Engine sender, Commands.Base command, Client client, Connection connection);
        public event TimeoutDelegate OnTimeout;
        public delegate void ProgressDelegate(Engine sender, Commands.Base command, Client client, Connection connection, DirectionType direction, int packetSize);
        public event ProgressDelegate OnProgress;
        public delegate void ErrorDelegate(Engine sender, Commands.Base command, Client client, string message, Exception exception);
        public event ErrorDelegate OnError;
        public delegate void CompletionDlegate(Engine sender, Commands.Base command, Client client, Connection connection, Response response);
        public event CompletionDlegate OnComplete;

        public Engine()
        {
        }

        //public void ExecuteCommand(Commands.Base cmd)
        //{
        //    cmd.Execute();
        //}
    }
}
