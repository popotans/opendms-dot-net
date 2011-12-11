using Http = OpenDMS.Networking.Protocols.Http;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutDocumentReply : ReplyBase
    {
        private const string _201 = "Document has been created successfully.";
        private const string _202 = "Document accepted for writing (batch mode).";
        private const string _409 = "Conflict - a document with the specified document ID already exists.";

        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public PutDocumentReply(Http.Response response)
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
                    Logger.Storage.Debug("PutDocumentReply loaded.");
                    break;
                case 202:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _202;
                    Ok = true;
                    Logger.Storage.Debug("PutDocumentReply loaded.");
                    break;
                case 409:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _409);
                    ResponseMessage = _409;
                    Ok = false;
                    Logger.Storage.Debug("PutDocumentReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("PutDocumentReply received an unknown response code: " + _response.StatusLine.StatusCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
