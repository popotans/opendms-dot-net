using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDatabaseReply : ReplyBase
    {
        private const string _200 = "Database has been deleted.";
        private const string _404 = "The requested content could not be found.";

        public bool Ok { get; set; }

        public DeleteDatabaseReply(Response response)
            : base(response)
        {            
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 200:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _200;
                    Ok = true;
                    Logger.Storage.Debug("DeleteDatabaseReply loaded.");
                    break;
                case 404:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _404);
                    ResponseMessage = _404;
                    Ok = false;
                    Logger.Storage.Debug("DeleteDatabaseReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("DeleteDatabaseReply received an unknown response code: " + _response.ResponseCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
