using System;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDatabase : Base
    {
        public DeleteDatabase(Uri uri)
            : base(uri, new Http.Methods.Delete())
        {
        }

        public override ReplyBase MakeReply(Http.Response response)
        {
            try
            {
                return new DeleteDatabaseReply(response);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the DeleteDatabaseReply.", e);
                throw;
            }
        }
    }
}
