using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Request 
        : Message.Base
    {
        public RequestLine RequestLine { get; set; }
    }
}
