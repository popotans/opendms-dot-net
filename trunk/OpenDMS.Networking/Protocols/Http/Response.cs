using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Response
        : Message.Base
    {
        public StatusLine StatusLine { get; set; }

        public Response()
            : base()
        {
            StatusLine = new StatusLine();
        }
    }
}
