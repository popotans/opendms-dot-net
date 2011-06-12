using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDocument : Base
    {
        public DeleteDocument(Uri uri)
            : base(new Delete(uri))
        {
        }

        public DeleteDocument(Delete delete)
            : base(delete)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new DeleteDocumentReply(response);
        }
    }
}
