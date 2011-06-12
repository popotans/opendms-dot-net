using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class HeadDocument : Base
    {
        public HeadDocument(Uri uri)
            : base(new Get(uri))
        {
        }

        public HeadDocument(Head head)
            : base(head)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new HeadDocumentReply(response);
        }
    }
}
