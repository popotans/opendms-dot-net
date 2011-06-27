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
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    ResponseMessage = _200;
                    string s = StringifyResponseStream();
                    try
                    {
                        View = new Model.View(s);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Storage.Error(s, e);
                        throw;
                    }
                    Ok = true;
                    Logger.Storage.Debug("GetViewReply loaded.");
                    break;
                case 400:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _404);
                    ResponseMessage = _404;
                    Ok = false;
                    Logger.Storage.Debug("GetViewReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("GetViewReply received an unknown response code: " + _response.ResponseCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
