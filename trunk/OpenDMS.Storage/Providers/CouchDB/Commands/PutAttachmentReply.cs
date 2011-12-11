using Http = OpenDMS.Networking.Protocols.Http;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutAttachmentReply : ReplyBase
    {
        private const string _201 = "Attachment has been accepted.";
        
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public PutAttachmentReply(Http.Response response)
            : base(response)
        {            
        }

        protected override void ParseResponse()
        {
            switch (_response.StatusLine.StatusCode)
            {
                case 201:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _201;
                    Ok = true;
                    JObject jobj = JObject.Parse(StringifyResponseStream());
                    Id = jobj["id"].Value<string>();
                    Rev = jobj["rev"].Value<string>();
                    Logger.Storage.Debug("PutAttachmentReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("PutAttachmentReply received an unknown response code: " + _response.StatusLine.StatusCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
