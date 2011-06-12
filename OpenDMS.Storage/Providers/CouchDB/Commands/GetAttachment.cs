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
            throw new NotImplementedException();
        }
    }
}
