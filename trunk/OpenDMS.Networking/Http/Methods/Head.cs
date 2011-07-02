using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Head : Request
    {
		#region Fields (1) 

        private const string METHOD = "HEAD";

		#endregion Fields 

		#region Constructors (1) 

        public Head(Uri uri)
            : base(uri)
        {
        }

		#endregion Constructors 

		#region Properties (1) 

        public override string Method { get { return METHOD; } }

		#endregion Properties 
    }
}
