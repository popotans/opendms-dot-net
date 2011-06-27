using System;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDatabase : Base
    {
        public PutDatabase(IDatabase db)
            : base(new Put(UriBuilder.Build(db), "application/json", 0))
        {
            _stream = null;
        }

        public override ReplyBase MakeReply(Response response)
        {
            try
            {
                return new PutDatabaseReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutDatabaseReply.", e);
                throw;
            }
        }
    }
}
