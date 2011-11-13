using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class RequestLine
    {
        public Methods.Base Method { get; set; }
        public Uri RequestUri { get; set; }
        public string HttpVersion { get; set; }

        public RequestLine()
        {
        }

        public static RequestLine Parse(string line)
        {
            RequestLine rl = new RequestLine();
            string method, requestUri, httpVersion;
            int loc;

            line = line.Trim();
            loc = line.IndexOf(" ");
            method = line.Substring(0, loc).Trim();
            line = line.Substring(loc).Trim();
            loc = line.IndexOf(" ");
            requestUri = line.Substring(0, loc).Trim();
            line = line.Substring(loc).Trim();
            httpVersion = line;

            switch (method)
            {
                case "DELETE":
                    rl.Method = new Methods.Delete();
                    break;
                case "GET":
                    rl.Method = new Methods.Get();
                    break;
                case "HEAD":
                    rl.Method = new Methods.Head();
                    break;
                case "POST":
                    rl.Method = new Methods.Post();
                    break;
                case "PUT":
                    rl.Method = new Methods.Put();
                    break;
                default:
                    throw new MethodParseException();
            }

            rl.RequestUri = new Uri(requestUri);
            rl.HttpVersion = httpVersion;

            return rl;
        }

        public override string ToString()
        {
            return Method.MethodName + " " + RequestUri.AbsolutePath + " " + HttpVersion;
        }
    }
}
