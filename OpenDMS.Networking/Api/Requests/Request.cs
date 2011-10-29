using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Requests
{
    public class Request<T> where T : RequestBase, new()
    {
        // Request = <number>\0<json><data>
        // <number> = long representing the length of the json content
        public static T Parse(HttpRequest request)
        {
            return Parse(request, 4192);
        }

        public static T Parse(HttpRequest request, int bufferSize)
        {
            CommandParser cp = new CommandParser(request.InputStream);
            cp.Execute();

            return new T() { FullContent = cp.Json, Stream = cp.Stream };
        }
    }
}
