using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Create : Base
    {
        public Create(Uri uri)
            : base(new Put(uri, "application/json"))
        {
        }

        public Create(Request put)
            : base(put)
        {
        }
    }
}
