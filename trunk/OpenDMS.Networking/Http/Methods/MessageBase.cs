using System.Net;

namespace OpenDMS.Networking.Http.Methods
{
    public class MessageBase
    {
        private WebHeaderCollection _headers = new WebHeaderCollection();

        public virtual WebHeaderCollection Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }
    }
}
