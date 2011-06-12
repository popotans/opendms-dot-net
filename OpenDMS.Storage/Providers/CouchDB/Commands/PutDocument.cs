using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDocument : Base
    {
        public PutDocument(Uri uri, ulong contentLength)
            : base(new Put(uri, "application/json", contentLength))
        {
        }

        public PutDocument(Put put)
            : base(put)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new PutDocumentReply(response);
        }
    }
}
