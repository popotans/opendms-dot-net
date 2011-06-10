using System.Net;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Error
    {
        public delegate void CreatedDelegate(Error sender);
        public event CreatedDelegate OnErrorCreated;

        protected Response _response = null;

        public virtual WebHeaderCollection Headers { get { return _response.Headers; } }

        public string ErrorType { get; private set; }
        public string Id { get; private set; }
        public string Reason { get; private set; }

        public Error(Response response)
        {
            response.Stream.OnTimeout += new HttpNetworkStream.TimeoutDelegate(Stream_OnTimeout);
            response.Stream.OnError += new HttpNetworkStream.ErrorDelegate(Stream_OnError);
            response.Stream.OnStringOperationComplete += new HttpNetworkStream.CompleteStringOperationDelegate(Stream_OnStringOperationComplete);
            
            response.Stream.ReadToEndAsync();
        }

        void Stream_OnStringOperationComplete(HttpNetworkStream sender, string result)
        {
            JObject jobj = JObject.Parse(result);

            if (jobj["error"] != null)
                ErrorType = jobj["error"].Value<string>();
            if (jobj["id"] != null)
                ErrorType = jobj["id"].Value<string>();
            if (jobj["reason"] != null)
                Reason = jobj["reason"].Value<string>();

            if (OnErrorCreated != null) OnErrorCreated(this);
            else throw new ErrorNotImplementedException("Error must be implemented.");
        }

        private void Stream_OnTimeout(HttpNetworkStream sender)
        {
            throw new System.NotImplementedException();
        }

        private void Stream_OnError(HttpNetworkStream sender, string message, System.Exception exception)
        {
            throw new System.NotImplementedException();
        }
    }
}
