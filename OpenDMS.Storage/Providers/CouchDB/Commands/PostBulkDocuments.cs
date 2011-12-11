using System;
using System.IO;
using System.Collections.Generic;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PostBulkDocuments : Base
    {
        public PostBulkDocuments(IDatabase db, Model.BulkDocuments documents)
            : base(UriBuilder.Build(db, documents), new Http.Methods.Post())
        {
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(documents.ToString()));
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new PostBulkDocumentsReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PostBulkDocumentsReply.", e);
                throw;
            }
        }
    }
}
