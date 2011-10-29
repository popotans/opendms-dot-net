using System;

namespace OpenDMS.Networking.Protocols.Http
{
    public class StatusLine
    {
        public string HttpVersion { get; set; }
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }

        public StatusLine()
        {
        }

        public static StatusLine Parse(string line)
        {
            StatusLine sl = new StatusLine();
            string httpVersion, statusCode, reasonPhrase;
            int loc;

            line = line.Trim();
            loc = line.IndexOf(" ");
            httpVersion = line.Substring(0, loc).Trim();
            line = line.Substring(loc).Trim();
            loc = line.IndexOf(" ");
            statusCode = line.Substring(0, loc).Trim();
            line = line.Substring(loc).Trim();
            reasonPhrase = line;

            sl.HttpVersion = httpVersion;
            sl.StatusCode = int.Parse(statusCode);
            sl.ReasonPhrase = reasonPhrase;

            return sl;
        }
    }
}
