using System.Net;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public abstract class ReplyBase
    {
        public delegate void CreatedDelegate(ReplyBase sender);
        public event CreatedDelegate OnReplyCreated;

        protected Response _response = null;

        public virtual WebHeaderCollection Headers { get { return _response.Headers; } }
        public Error Error { get; protected set; }

        public ReplyBase(Response response)
        {
            _response = response;
            if (_response.ResponseCode >= 400 && _response.ResponseCode < 500)
            {
                Error.OnErrorCreated += new Commands.Error.CreatedDelegate(Error_OnErrorCreated);
                Error = new Error(response);
            }
            else
                ParseResponse();
        }

        protected abstract void ParseResponse();

        private void Error_OnErrorCreated(Error sender)
        {
            if (OnReplyCreated != null) OnReplyCreated(this);
        }

        public void ReportReplyCreated(ReplyBase sender)
        {
            if (OnReplyCreated != null) OnReplyCreated(sender);
        }
    }
}
