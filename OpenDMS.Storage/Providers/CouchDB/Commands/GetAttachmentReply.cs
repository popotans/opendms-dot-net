using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetAttachmentReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _404 = "The requested attachment could not be found or has been deleted.";

        public bool Ok { get; private set; }
        public string ContentType { get; private set; }
        public ulong Length { get; private set; }
        public Http.HttpNetworkStream Stream { get; private set; }

        public GetAttachmentReply(Http.Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.StatusLine.StatusCode)
            {
                case 200:
                    if (_response.Body.ReceiveStream.GetType() != typeof(Networking.Protocols.Http.HttpNetworkStream))
                        throw new OpenDMS.Networking.Protocols.Http.HttpNetworkStreamException("Invalid stream type.");

                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _200;
                    ContentType = _response.ContentType;
                    if (!_response.ContentLength.HasValue)
                        throw new Http.Message.HeaderException("Content-Length header does not exist.");
                    Length = (ulong)_response.ContentLength.Value;
                    Stream = (Networking.Protocols.Http.HttpNetworkStream)_response.Body.ReceiveStream;
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
                    Logger.Storage.Error("GetAttachmentReply received an unknown response code: " + _response.StatusLine.StatusCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
