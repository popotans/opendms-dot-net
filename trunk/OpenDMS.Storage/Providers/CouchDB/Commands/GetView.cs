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
            return new GetViewReply(response);
        }
    }
}
