using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDatabase : Base
    {
        public PutDatabase(IDatabase db)
            : base(UriBuilder.Build(db), new Http.Methods.Put())
        {
            _stream = null;
        }

        public override ReplyBase MakeReply(Http.Response response)
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
