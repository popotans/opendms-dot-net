using System;
using System.Web;
using Newtonsoft.Json.Linq;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Networking.Api.Requests
{
    public class RequestBase : MessageBase
    {
        public RequestBase()
        {
        }

        public RequestBase(JObject fullContent)
            : base(fullContent)
        {
            FullContent = fullContent;
        }

        public virtual Protocols.Http.Methods.Base GetMethod()
        {
            throw new NotImplementedException("GetMethod must be implemented.");
        }

        public virtual Protocols.Http.Request CreateRequest(Uri uri, string contentType)
        {
            throw new NotImplementedException("CreateRequest must be implemented.");
        }

        protected Protocols.Http.Request CreateRequest(Protocols.Http.Methods.Base method, Uri uri,
            string contentType)
        {
            Protocols.Http.Request request;
            MultisourcedStream fullStream;
            long fullContentLength;

            request = new Protocols.Http.Request(method, uri);

            fullStream = MakeStream(out fullContentLength);

            request.Headers.Add(new Protocols.Http.Message.ContentLengthHeader(fullContentLength));

            request.Body.IsChunked = false;
            request.Body.SendStream = fullStream;

            return request;
        }

        protected static Requests.RequestBase Parse(HttpRequest dotNetRequest)
        {
            Requests.RequestBase apiRequest;
            Http.Request request;
            byte[] buffer = new byte[8192];
            int bytesRead = 0;
            int bytesToRead = 0;
            int byteValue = 0;
            int jsonLength = 0;
            int remainingJsonLength = 0;
            string temp = "";

            // Instantiate our Http.Request
            request = new Http.Request(dotNetRequest);

            // read byte by byte until we hit null
            // We are building our json length
            while ((byteValue = request.Body.ReceiveStream.ReadByte()) > 0)
            {
                temp += byteValue.ToString();
            }

            jsonLength = int.Parse(temp);
            remainingJsonLength = jsonLength;
            temp = "";

            if (jsonLength > 8192)
                bytesToRead = 8192;
            else
                bytesToRead = jsonLength;

            // We now need to read through the body to the end of the Json
            while ((bytesRead = request.Body.ReceiveStream.Read(buffer, 0, bytesToRead)) > 0)
            {
                temp += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                remainingJsonLength -= bytesRead;

                if (remainingJsonLength <= 0)
                    break;
                else if (remainingJsonLength > 8192)
                    bytesToRead = 8192;
                else
                    bytesToRead = remainingJsonLength;
            }

            // all that would remain in the stream would be data
            apiRequest = new Requests.RequestBase(JObject.Parse(temp))
            {
                Stream = request.Body.ReceiveStream
            };

            return apiRequest;
        }
    }
}
