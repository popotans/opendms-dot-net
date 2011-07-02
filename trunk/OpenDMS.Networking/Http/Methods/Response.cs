using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Response : MessageBase
    {
		#region Constructors (1) 

        public Response()
        {
        }

		#endregion Constructors 

		#region Properties (2) 

        public int ResponseCode { get; set; }

        public HttpNetworkStream Stream { get; set; }

		#endregion Properties 
    }
}
