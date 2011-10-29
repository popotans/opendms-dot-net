using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api.Responses
{
    public class Response<T> where T : ResponseBase, new()
    {
        public static T Parse(Http.Methods.Response response)
        {
            return Parse(response, 4192);
        }

        public static T Parse(Http.Methods.Response response, int bufferSize)
        {
            CommandParser cp = new CommandParser(response.Stream);
            cp.Execute();

            return new T() { FullContent = cp.Json, NetworkStream = cp.NetworkStream };
        }
    }
}
