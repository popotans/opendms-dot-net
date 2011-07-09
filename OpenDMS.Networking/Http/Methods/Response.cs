using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Response : MessageBase
    {
        public Request Request { get; private set; }
		#region Constructors (1) 

        public Response(Request request)
        {
            Request = request;
        }

		#endregion Constructors 

		#region Properties (2) 

        public int ResponseCode { get; set; }

        public HttpNetworkStream Stream { get; set; }

		#endregion Properties 
    }
}
