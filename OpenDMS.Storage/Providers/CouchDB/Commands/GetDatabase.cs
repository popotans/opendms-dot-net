using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDatabase : Base
    {
        public GetDatabase(Uri uri)
            : base(new Get(uri))
        {
        }

        public GetDatabase(Get get)
            : base(get)
        {
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new GetDatabaseReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDatabaseReply.", e);
                throw;
            }
        }
    }
}
