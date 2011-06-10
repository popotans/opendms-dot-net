using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class List : Base
    {
        public List(Uri uri)
            : base(new OpenDMS.Networking.Http.Methods.Get(uri))
        {
        }

        public List(Request get)
            : base(get)
        {
        }
    }
}
