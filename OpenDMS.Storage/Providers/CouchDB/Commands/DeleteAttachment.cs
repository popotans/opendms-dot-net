using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteAttachment : Base
    {
        public DeleteAttachment(Uri uri)
            : base(new Delete(uri))
        {
        }

        public DeleteAttachment(Delete delete)
            : base(delete)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new DeleteAttachmentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the DeleteAttachmentReply.", e);
                throw;
            }
        }
    }
}
