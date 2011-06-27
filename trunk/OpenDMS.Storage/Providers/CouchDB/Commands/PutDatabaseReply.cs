using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDatabaseReply : ReplyBase
    {
        private const string _201 = "Database created successfully.";
        private const string _400 = "Invalid database name.";
        private const string _412 = "Database already exists.";

        public bool Ok { get; set; }

        public PutDatabaseReply(Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 201:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _201;
                    Ok = true;
                    Logger.Storage.Debug("PutDatabaseReply loaded.");
                    break;
                case 400:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _400);
                    ResponseMessage = _400;
                    Ok = false;
                    Logger.Storage.Debug("PutDatabaseReply loaded.");
                    break;
                case 412:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _412);
                    ResponseMessage = _412;
                    Ok = false;
                    Logger.Storage.Debug("PutDatabaseReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("PutDatabaseReply received an unknown response code: " + _response.ResponseCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
