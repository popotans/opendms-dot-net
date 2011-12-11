using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class HeadDocument : Base
    {
        public HeadDocument(Uri uri)
            : base(uri, new Http.Methods.Head())
        {
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new HeadDocumentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the HeadDocumentReply.", e);
                throw;
            }
        }
    }
}
