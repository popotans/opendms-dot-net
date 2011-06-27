using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetAttachmentReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _404 = "The requested attachment could not be found or has been deleted.";

        public bool Ok { get; private set; }
        public string ContentType { get; private set; }
        public ulong Length { get; private set; }
        public Networking.Http.HttpNetworkStream Stream { get; private set; }

        public GetAttachmentReply(Response response)
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
                    ContentType = OpenDMS.Networking.Utilities.GetContentType(_response.Headers);
                    Length = OpenDMS.Networking.Utilities.GetContentLength(_response.Headers);
                    Stream = _response.Stream;
                    Ok = true;
                    Logger.Storage.Debug("GetAttachmentReply loaded.");
                    break;
                case 404:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _404);
                    ResponseMessage = _404;
                    Ok = false;
                    Logger.Storage.Debug("GetAttachmentReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("GetAttachmentReply received an unknown response code: " + _response.ResponseCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
