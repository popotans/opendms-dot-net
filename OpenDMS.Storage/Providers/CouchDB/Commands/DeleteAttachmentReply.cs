using Http = OpenDMS.Networking.Protocols.Http;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteAttachmentReply : ReplyBase
    {
        private const string _200 = "Attachment has been deleted.";
        private const string _409 = "Revision is missing, invalid or not the latest.";

        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public DeleteAttachmentReply(Http.Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.StatusLine.StatusCode)
            {
                case 200:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _200;
                    Ok = true;
                    JObject jobj = JObject.Parse(StringifyResponseStream());
                    Id = jobj["id"].Value<string>();
                    Rev = jobj["rev"].Value<string>();
                    Logger.Storage.Debug("DeleteAttachmentReply loaded.");
                    break;
                case 409:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _409);
                    ResponseMessage = _409;
                    Ok = false;
                    Logger.Storage.Debug("DeleteAttachmentReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("DeleteAttachmentReply received an unknown response code: " + _response.StatusLine.StatusCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
