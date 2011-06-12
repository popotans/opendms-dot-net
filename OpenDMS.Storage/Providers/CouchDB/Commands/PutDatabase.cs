using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDatabase : Base
    {
        public PutDatabase(Uri uri)
            : base(new Put(uri, "application/json"))
        {
        }

        public PutDatabase(Put put)
            : base(put)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new PutDatabaseReply(response);
        }
    }
}
