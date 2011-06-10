using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Delete : Base
    {
        public Delete(Uri uri)
            : base(new OpenDMS.Networking.Http.Methods.Delete(uri))
        {
        }

        public Delete(Request delete)
            : base(delete)
        {
        }
    }
}
