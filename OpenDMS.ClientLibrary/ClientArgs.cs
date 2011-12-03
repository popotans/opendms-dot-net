using System;
using Api = OpenDMS.Networking.Api;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.ClientLibrary
{
    public class ClientArgs
    {
        public Uri Uri { get; set; }
        public string ContentType { get; set; }
        //public long ContentLength { get; set; }

        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
        public int SendBufferSize { get; set; }
        public int ReceiveBufferSize { get; set; }

        public Client.ConnectionDelegate OnConnect { get; set; }
        public Client.ConnectionDelegate OnDisconnect { get; set; }
        public Client.CompletionDelegate OnComplete { get; set; }
        public Client.ErrorDelegate OnError { get; set; }
        public Client.ProgressDelegate OnProgress { get; set; }
        public Client.ConnectionDelegate OnTimeout { get; set; }
    }
}
