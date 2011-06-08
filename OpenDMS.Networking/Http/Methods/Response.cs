using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Response : MessageBase
    {
        public int ResponseCode { get; set; }
        public HttpNetworkStream Stream { get; set; }

        public Response()
        {
        }
    }
}
