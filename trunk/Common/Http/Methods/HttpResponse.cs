using System;

namespace Common.Http.Methods
{
    public class HttpResponse : HttpMessageBase
    {
        public int ResponseCode { get; set; }
        public Network.HttpNetworkStream Stream { get; set; }

        public HttpResponse()
        {
        }
    }
}
