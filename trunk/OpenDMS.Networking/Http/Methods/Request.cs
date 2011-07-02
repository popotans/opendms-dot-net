using System;
using System.Net;

namespace OpenDMS.Networking.Http.Methods
{
    public abstract class Request : MessageBase
    {
		#region Fields (1) 

        private Uri _uri = null;

		#endregion Fields 

		#region Constructors (1) 

        public Request(Uri uri)
        {
            _uri = uri;
        }

		#endregion Constructors 

		#region Properties (5) 

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

        //private string _method = null;
        public abstract string Method { get; }

        public virtual string RequestLine
        {
            get
            {
                return Method + " " + Uri.PathAndQuery + " HTTP/1.1";
            }
        }

        public Uri Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

		#endregion Properties 
    }
}
