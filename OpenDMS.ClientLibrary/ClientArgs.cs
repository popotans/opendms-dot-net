using System;
using Http = OpenDMS.Networking.Http;

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

        public Http.Client.CloseDelegate OnClose { get; set; }
        public Http.Client.CompletionDelegate OnComplete { get; set; }
        public Http.Client.ErrorDelegate OnError { get; set; }
        public Http.Client.ProgressDelegate OnProgress { get; set; }
        public Http.Client.TimeoutDelegate OnTimeout { get; set; }
    }
}
