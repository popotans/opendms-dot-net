using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetView : Base
    {
        public GetView(Uri uri)
            : base(new Get(uri))
        {
        }

        public GetView(Get get)
            : base(get)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new GetViewReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetViewReply.", e);
                throw;
            }
        }
    }
}
