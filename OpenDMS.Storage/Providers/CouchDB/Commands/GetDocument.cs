using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDocument : Base
    {
        public GetDocument(Uri uri)
            : base(new Get(uri))
        {
        }

        public GetDocument(Get get)
            : base(get)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new GetDocumentReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocumentReply.", e);
                throw;
            }
        }
    }
}
