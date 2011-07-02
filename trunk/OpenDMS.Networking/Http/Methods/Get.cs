using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Get : Request
    {
		#region Fields (1) 

        private const string METHOD = "GET";

		#endregion Fields 

		#region Constructors (1) 

        public Get(Uri uri)
            : base(uri)
        {
        }

		#endregion Constructors 

		#region Properties (1) 

        public override string Method { get { return METHOD; } }

		#endregion Properties 
    }
}
