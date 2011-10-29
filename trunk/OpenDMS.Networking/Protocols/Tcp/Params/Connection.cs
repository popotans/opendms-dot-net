using System;
using System.Net;

namespace OpenDMS.Networking.Protocols.Tcp.Params
{
    public class Connection
    {
        public IPEndPoint EndPoint { get; set; }
        public Buffer ReceiveBuffer { get; set; }
        public Buffer SendBuffer { get; set; }
    }
}
