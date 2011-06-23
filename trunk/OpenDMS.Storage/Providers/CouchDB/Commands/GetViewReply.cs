using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetViewReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _404 = "The requested view could not be found or has been deleted.";

        public bool Ok { get; private set; }
        public Model.View View { get; private set; }

        public GetViewReply(Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 200:
                    ResponseMessage = _200;
                    View = new Model.View(StringifyResponseStream());
                    Ok = true;
                    break;
                case 400:
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
