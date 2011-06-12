using OpenDMS.Networking.Http.Methods;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PutAttachmentReply : ReplyBase
    {
        private const string _201 = "Attachment has been accepted.";
        
        public bool Ok { get; set; }
        public string Id { get; set; }
        public string Rev { get; set; }

        public PutAttachmentReply(Response response)
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
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
