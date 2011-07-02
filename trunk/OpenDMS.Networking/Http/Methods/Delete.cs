using System;

namespace OpenDMS.Networking.Http.Methods
{
    public class Delete : Request
    {
		#region Fields (1) 

        private const string METHOD = "DELETE";

		#endregion Fields 

		#region Constructors (1) 

        public Delete(Uri uri)
            : base(uri)
        {
        }

		#endregion Constructors 

		#region Properties (1) 

        public override string Method { get { return METHOD; } }

		#endregion Properties 
    }
}
