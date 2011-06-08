using System;
using System.Net;

namespace OpenDMS.Networking.Http.Methods
{
    public abstract class Request : MessageBase
    {
        private Uri _uri = null;
        private string _method = null;

        public abstract string Method { get; }
        
        public virtual string RequestLine
        {
            get
            {
                return Method + " " + Uri.PathAndQuery + " HTTP/1.1";
            }
        }

        public virtual string ContentType
        {
            get
            {
                return Headers[HttpRequestHeader.ContentType];
            }
            set
            {
                Headers[HttpRequestHeader.ContentType] = value;
            }
        }

        public virtual string ContentLength
        {
            get
            {
                return Headers[HttpRequestHeader.ContentLength];
            }
            set
            {
                Headers[HttpRequestHeader.ContentLength] = value;
            }
        }


        public Uri Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        public Request(Uri uri)
        {
            _uri = uri;
        }
    }
}
