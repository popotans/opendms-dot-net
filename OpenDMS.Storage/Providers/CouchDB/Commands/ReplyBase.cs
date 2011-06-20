using System.Net;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class ReplyBase : ICommandReply
    {
        protected Response _response = null;

        public virtual WebHeaderCollection Headers { get { return _response.Headers; } }
        public Error Error { get; protected set; }
        public string ResponseMessage { get; protected set; }

        public ReplyBase(Response response)
        {
            _response = response;
            if (_response.ResponseCode >= 400 && _response.ResponseCode < 500)
                Error = new Error(response);
            else
                ParseResponse();
        }

        protected abstract void ParseResponse();
        protected string StringifyResponseStream()
        {
            return _response.Stream.ReadToEnd();
        }

        public bool IsError
        {
            get { return Error != null; }
        }

        public string ErrorMessage
        {
            get 
            { 
                if (Error != null) return Error.Reason;
                return null;
            }
        }
    }
}
