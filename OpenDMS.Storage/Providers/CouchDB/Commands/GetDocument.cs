using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDocument : Base
    {
        public GetDocument(Uri uri)
            : base(new Get(uri))
        {
        }

        public GetDocument(Get get)
            : base(get)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new GetDocumentReply(response);
        }
    }
}
