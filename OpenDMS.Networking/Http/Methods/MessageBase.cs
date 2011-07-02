using System.Net;

namespace OpenDMS.Networking.Http.Methods
{
    public class MessageBase
    {
		#region Fields (1) 

        private WebHeaderCollection _headers = new WebHeaderCollection();

		#endregion Fields 

		#region Properties (1) 

        public virtual WebHeaderCollection Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

		#endregion Properties 
    }
}
