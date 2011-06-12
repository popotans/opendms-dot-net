using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutAttachment : Base
    {
        public PutAttachment(Uri uri, string contentType, ulong contentLength)
            : base(new Put(uri, contentType, contentLength))
        {
        }

        public PutAttachment(Put put)
            : base(put)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new PutAttachmentReply(response);
        }
    }
}
