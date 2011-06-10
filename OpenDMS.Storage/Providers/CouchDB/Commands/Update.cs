using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Update : Base
    {
        public Update(Uri uri)
            : base(new Put(uri, "application/json"))
        {
        }

        public Update(Request put)
            : base(put)
        {
        }
    }
}
