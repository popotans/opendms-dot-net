using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDatabase : Base
    {
        public DeleteDatabase(Uri uri)
            : base(new Delete(uri))
        {
        }

        public DeleteDatabase(Delete delete)
            : base(delete)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new DeleteDatabaseReply(response);
        }
    }
}
