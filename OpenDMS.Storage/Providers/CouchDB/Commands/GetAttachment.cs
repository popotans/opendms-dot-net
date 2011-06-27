using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetAttachment : Base
    {
        public GetAttachment(Uri uri)
            : base(new Get(uri))
        {
        }

        public GetAttachment(Get get)
            : base(get)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new GetAttachmentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetAttachmentReply.", e);
                throw;
            }
        }
    }
}
