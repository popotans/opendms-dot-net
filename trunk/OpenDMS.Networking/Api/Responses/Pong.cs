using System;
using System.Web;

namespace OpenDMS.Networking.Api.Responses
{
    public class Pong : ResponseBase
    {
        public static new ResponseBase BuildFrom(Requests.RequestBase request)
        {
            Responses.Pong pong = new Responses.Pong();
            return pong;
        }

        public static new ResponseBase BuildFrom(Requests.RequestBase request, params object[] obj)
        {
            return BuildFrom(request);
        }
    }
}
