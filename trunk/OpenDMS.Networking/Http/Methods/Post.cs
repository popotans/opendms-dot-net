using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Post : Request
    {
		#region Fields (1) 

        private const string METHOD = "POST";

		#endregion Fields 

		#region Constructors (2) 

        public Post(Uri uri, string contentType, ulong contentLength)
            : base(uri)
        {
            ContentType = contentType;
            ContentLength = contentLength.ToString();
        }

        public Post(Uri uri, string contentType)
            : base(uri)
        {
            ContentType = contentType;
        }

		#endregion Constructors 

		#region Properties (1) 

        public override string Method { get { return METHOD; } }

		#endregion Properties 
    }
}
