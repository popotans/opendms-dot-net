using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class HeadAttachment : Base
    {
        public HeadAttachment(Uri uri)
            : base(new Get(uri))
        {
        }

        public HeadAttachment(Head head)
            : base(head)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new HeadAttachmentReply(response);
        }
    }
}
