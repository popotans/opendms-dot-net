using System;
using System.IO;
using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PostBulkDocuments : Base
    {
        public PostBulkDocuments(IDatabase db, Model.BulkDocuments documents)
            : base(new Post(UriBuilder.Build(db, documents), "application/json", documents.Length))
        {
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(documents.ToString()));
        }

        public override ReplyBase MakeReply(Response response)
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
