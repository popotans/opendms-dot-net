using System;
using System.IO;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDocument : Base
    {
        public PutDocument(IDatabase db, Model.Document doc)
            : base(UriBuilder.Build(db, doc), new Http.Methods.Put())
        {
            _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(doc.ToString()));
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new PutDocumentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutDocumentReply.", e);
                throw;
            }
        }
    }
}
