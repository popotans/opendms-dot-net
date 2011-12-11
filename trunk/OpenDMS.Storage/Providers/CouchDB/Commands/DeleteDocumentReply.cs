using Http = OpenDMS.Networking.Protocols.Http;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class DeleteDocumentReply : ReplyBase
    {
        private const string _200 = "Document has been deleted.";
        private const string _409 = "Revision is missing, invalid or not the latest.";

        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public DeleteDocumentReply(Http.Response response)
            : base(response)
        {            
        }

        protected override void ParseResponse()
        {
            switch (_response.StatusLine.StatusCode)
            {
                case 200:
                    ResponseMessage = _200;
                    Ok = true;
                    JObject jobj = JObject.Parse(StringifyResponseStream());
                    Id = jobj["id"].Value<string>();
                    Rev = jobj["rev"].Value<string>();
                    break;
                case 409:
                    ResponseMessage = _409;
                    Ok = false;
                    break;
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
