using System.Net;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class ReplyBase : ICommandReply
    {
        protected Http.Response _response = null;

        public virtual Http.Message.HeaderCollection Headers { get { return _response.Headers; } }
        public Error Error { get; protected set; }
        public string ResponseMessage { get; protected set; }

        public ReplyBase(Http.Response response)
        {
            _response = response;
            if (_response.StatusLine.StatusCode >= 400 && _response.StatusLine.StatusCode < 500)
                Error = new Error(response);
            else
                ParseResponse();
        }

        protected abstract void ParseResponse();
        protected string StringifyResponseStream()
        {
            if (_response.Body.ReceiveStream == null)
                return null;

            // Chunked uses InterceptorStream 

            if (_response.Body.ReceiveStream.GetType() != typeof(Networking.Protocols.Http.HttpNetworkStream))
            {
                Logger.Storage.Error("Need to implement interceptors for chunked encoding.");
                throw new OpenDMS.Networking.Protocols.Http.HttpNetworkStreamException("Invalid stream type.");
            }

            // Chunked cannot read to end in current state
            return ((Networking.Protocols.Http.HttpNetworkStream)_response.Body.ReceiveStream).ReadToEnd();
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
