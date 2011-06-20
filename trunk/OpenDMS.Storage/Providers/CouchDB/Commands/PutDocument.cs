using System;
using System.IO;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDocument : Base
    {
        public PutDocument(IDatabase db, Model.Document doc)
            : base(new Put(UriBuilder.Build(db, doc), "application/json", doc.Length))
        {
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(doc.ToString()));
        }

        public override ReplyBase MakeReply(Response response)
        {
            return new PutDocumentReply(response);
        }
    }
}
