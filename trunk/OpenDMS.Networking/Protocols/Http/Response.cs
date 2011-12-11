using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Response
        : Message.Base
    {
        public Request Request { get; private set; }
        public StatusLine StatusLine { get; set; }

        public Response(Request request)
            : base()
        {
            Request = request;
            StatusLine = new StatusLine();
        }
    }
}
