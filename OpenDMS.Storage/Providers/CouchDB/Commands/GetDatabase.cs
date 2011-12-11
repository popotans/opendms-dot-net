using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDatabase : Base
    {
        public GetDatabase(Uri uri)
            : base(uri, new Http.Methods.Get())
        {
        }

        public override ReplyBase MakeReply(Http.Response response)
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
