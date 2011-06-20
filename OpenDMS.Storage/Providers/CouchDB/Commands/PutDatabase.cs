using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDatabase : Base
    {
        public PutDatabase(IDatabase db)
            : base(new Put(UriBuilder.Build(db), "application/json", 0))
        {
            _stream = null;
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new PutDatabaseReply(response);
        }
    }
}
