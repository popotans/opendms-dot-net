using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Get : Base
    {
        public Get(Uri uri)
            : base(new OpenDMS.Networking.Http.Methods.Get(uri))
        {
        }

        public Get(Request get)
            : base(get)
        {
        }
    }
}
