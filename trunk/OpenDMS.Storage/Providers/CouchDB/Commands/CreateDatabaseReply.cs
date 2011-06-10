using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class CreateDatabaseReply : ReplyBase
    {
        private const string _201 = "Database created successfully.";
        private const string _400 = "Invalid database name.";
        private const string _412 = "Database already exists.";
        private const string _att_201 = "Attachment has been accepted.";

        public bool Ok { get; set; }
        public string ResponseMessage { get; private set; }

        public CreateDatabaseReply(Response response)
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
                    break;
                case 400:
                    ResponseMessage = _400;
                    Ok = false;
                    break;
                case 412:
                    ResponseMessage = _412;
                    Ok = false;
                    break;
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }

            ReportReplyCreated(this);
        }
    }
}
