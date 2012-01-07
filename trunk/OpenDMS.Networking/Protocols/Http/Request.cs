using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http
{
    public class Request 
        : Message.Base
    {
        public RequestLine RequestLine { get; set; }

        public Request(Methods.Base method, Uri uri)
        {
            RequestLine = new RequestLine() { HttpVersion = "HTTP/1.1", Method = method, RequestUri = uri };
        }

        public Request(System.Web.HttpRequest request)
        {
            Headers = new Message.HeaderCollection(request.Headers);
            RequestLine = new RequestLine()
            {
                HttpVersion = "HTTP/1.1",
                Method = Methods.Base.Parse(request.HttpMethod),
                RequestUri = request.Url
            };
            Body = new Message.Body() 
            { 
                IsChunked = false, 
                ReceiveStream = request.InputStream 
            };
        }

        public MemoryStream MakeRequestLineAndHeadersStream()
        {
            Message.HostHeader hostHeader;

            hostHeader = new Message.HostHeader(this.RequestLine.RequestUri.Host);
            Headers[hostHeader.Name] = hostHeader.Value;

            if (RequestLine.Method.GetType() == typeof(Http.Methods.Get))
            {

            }
            else if (RequestLine.Method.GetType() == typeof(Http.Methods.Delete))
            {

            }
            else if (RequestLine.Method.GetType() == typeof(Http.Methods.Head))
            {

            }
            else
            {
                // Currently not supporting sending of chunked content
                if (ContentLength == null)
                    throw new Message.HeaderException("Content-Length header is null");
            }

            return new MemoryStream(System.Text.Encoding.ASCII.GetBytes(RequestLine.ToString() + "\r\n" + Headers.ToString() + "\r\n"));
        }

        public MemoryStream MakeRequestLineAndHeadersStream(string postpend)
        {
            Message.HostHeader hostHeader;

            hostHeader = new Message.HostHeader(this.RequestLine.RequestUri.Host);
            Headers[hostHeader.Name] = hostHeader.Value;

            if (RequestLine.Method.GetType() == typeof(Http.Methods.Get))
            {

            }
            else if (RequestLine.Method.GetType() == typeof(Http.Methods.Delete))
            {

            }
            else if (RequestLine.Method.GetType() == typeof(Http.Methods.Head))
            {

            }
            else
            {
                // Currently not supporting sending of chunked content
                if (ContentLength == null)
                    throw new Message.HeaderException("Content-Length header is null");
            }

            return new MemoryStream(System.Text.Encoding.ASCII.GetBytes(RequestLine.ToString() + "\r\n" + Headers.ToString() + "\r\n" + postpend));
        }
    }
}
