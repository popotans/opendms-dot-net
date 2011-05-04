using System;
using System.Net;

namespace Common.Http.Methods
{
    public class HttpMessageBase
    {
        private WebHeaderCollection _headers = new WebHeaderCollection();

        public virtual WebHeaderCollection Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }
    }
}
