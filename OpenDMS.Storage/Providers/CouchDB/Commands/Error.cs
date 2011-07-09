using System.Net;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class Error
    {
        //public delegate void CreatedDelegate(Error sender);
        //public event CreatedDelegate OnErrorCreated;

        protected Response _response = null;

        public virtual WebHeaderCollection Headers { get { return _response.Headers; } }

        public string ErrorType { get; private set; }
        public string Id { get; private set; }
        public string Reason { get; private set; }

        public Error(Response response)
        {
            //response.Stream.OnTimeout += new HttpNetworkStream.TimeoutDelegate(Stream_OnTimeout);
            //response.Stream.OnError += new HttpNetworkStream.ErrorDelegate(Stream_OnError);
            //response.Stream.OnStringOperationComplete += new HttpNetworkStream.CompleteStringOperationDelegate(Stream_OnStringOperationComplete);

            Logger.Storage.Debug("Forming error package...");
            
            //response.Stream.ReadToEndAsync();

            try
            {
                // Head method receives a content length but does not receive content, so we would wait a timeout to get a response
                // causing all kinds of problems.
                if (response.Request.GetType() != typeof(OpenDMS.Networking.Http.Methods.Head))
                {
                    string str = response.Stream.ReadToEnd();
                    if (str != null && str != "")
                    {
                        JObject jobj = JObject.Parse(str);

                        if (jobj["error"] != null)
                            ErrorType = jobj["error"].Value<string>();
                        if (jobj["id"] != null)
                            Id = jobj["id"].Value<string>();
                        if (jobj["reason"] != null)
                            Reason = jobj["reason"].Value<string>();
                    }
                    else
                    {
                        ErrorType = "unknown";
                        Id = null;
                        Reason = "Unknown";
                    }
                }
                else
                {
                    ErrorType = "unknown";
                    Id = null;
                    Reason = "Unknown";
                }
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the Error object.", e);
                throw;
            }

            Logger.Storage.Debug("Error package loaded.");
        }

        //void Stream_OnStringOperationComplete(HttpNetworkStream sender, string result)
        //{
        //    try
        //    {
        //        JObject jobj = JObject.Parse(result);

        //        if (jobj["error"] != null)
        //            ErrorType = jobj["error"].Value<string>();
        //        if (jobj["id"] != null)
        //            ErrorType = jobj["id"].Value<string>();
        //        if (jobj["reason"] != null)
        //            Reason = jobj["reason"].Value<string>();
        //    }
        //    catch (System.Exception e)
        //    {
        //        Logger.Storage.Error("An exception occurred while creating the Error object.", e);
        //        throw;
        //    }

        //    Logger.Storage.Debug("Error package loaded.");
        //    if (OnErrorCreated != null) OnErrorCreated(this);
        //}

        //private void Stream_OnTimeout(HttpNetworkStream sender)
        //{
        //    Logger.Storage.Debug("Timeout occurred while forming error package.");
        //    throw new System.NotImplementedException();
        //}

        //private void Stream_OnError(HttpNetworkStream sender, string message, System.Exception exception)
        //{
        //    Logger.Storage.Debug("An error occurred while forming error package.  Message: " + message, exception);
        //    throw new System.NotImplementedException();
        //}
    }
}
