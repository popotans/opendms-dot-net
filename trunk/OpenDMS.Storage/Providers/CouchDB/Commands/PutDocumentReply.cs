using OpenDMS.Networking.Http.Methods;
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

        public PutDocumentReply(Response response)
            : base(response)
        {            
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 201:
                    ResponseMessage = _201;
                    Ok = true;
                    JObject jobj = JObject.Parse(StringifyResponseStream());
                    Id = jobj["id"].Value<string>();
                    Rev = jobj["rev"].Value<string>();
                    break;
                case 202:
                    ResponseMessage = _202;
                    Ok = true;
                    break;
                case 409:
                    ResponseMessage = _409;
                    Ok = false;
                    break;
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
