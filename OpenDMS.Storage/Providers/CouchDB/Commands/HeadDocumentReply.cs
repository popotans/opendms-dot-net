using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class HeadDocumentReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _404 = "The requested document or revision could not be found or has been deleted.";

        public bool Ok { get; private set; }

        public HeadDocumentReply(Response response)
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
                    Logger.Storage.Debug("HeadDocumentReply loaded.");
                    break;
                case 404:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _404);
                    ResponseMessage = _404;
                    Ok = false;
                    Logger.Storage.Debug("HeadDocumentReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("HeadDocumentReply received an unknown response code: " + _response.ResponseCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
