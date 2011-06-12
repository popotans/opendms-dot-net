using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDocumentReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _400 = "The format of the request or revision was invalid";
        private const string _404 = "The requested document or revision could not be found or has been deleted.";

        public bool Ok { get; private set; }
        public Model.Document Document { get; private set; }

        public GetDocumentReply(Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 200:
                    ResponseMessage = _200;
                    Document = new Model.Document(StringifyResponseStream());
                    Ok = true;
                    break;
                case 400:
                    ResponseMessage = _400;
                    Ok = false;
                    break;
                case 404:
                    ResponseMessage = _404;
                    Ok = false;
                    break;
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
