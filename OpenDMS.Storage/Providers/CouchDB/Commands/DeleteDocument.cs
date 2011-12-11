using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDocument : Base
    {
        public DeleteDocument(Uri uri)
            : base(uri, new Http.Methods.Delete())
        {
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new DeleteDocumentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the DeleteDocumentReply.", e);
                throw;
            }
        }
    }
}
