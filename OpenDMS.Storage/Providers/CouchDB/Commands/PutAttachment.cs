using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutAttachment : Base
    {
        public PutAttachment(IDatabase db, Model.Document document, string attachmentName, Model.Attachment attachment, System.IO.Stream stream)
            : base(UriBuilder.Build(db, document, attachmentName), new Http.Methods.Delete())
        {
            _stream = stream;
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new PutAttachmentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutAttachmentReply.", e);
                throw;
            }
        }
    }
}
