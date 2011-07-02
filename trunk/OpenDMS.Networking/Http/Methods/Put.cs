using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Put : Request
    {
		#region Fields (1) 

        private const string METHOD = "PUT";

		#endregion Fields 

		#region Constructors (2) 

        public Put(Uri uri, string contentType, ulong contentLength)
            : base(uri)
        {
            ContentType = ContentType;
            ContentLength = contentLength.ToString();
        }

        public Put(Uri uri, string contentType)
            : base(uri)
        {
            ContentType = ContentType;
        }

		#endregion Constructors 

		#region Properties (1) 

        public override string Method { get { return METHOD; } }

		#endregion Properties 
    }
}
