using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class Body
    {
        public bool IsChunked { get; set; }
        public System.IO.Stream ReceiveStream { get; set; }
        // Dot Net receives using the basic "Stream" class and we have no way to access
        // the underlying socket, so we must use this... 'way'.
        //public System.IO.Stream DotNetReceiveStream { get; set; }
        public System.IO.Stream SendStream { get; set; }
    }
}
