using System;
using System.Web;

namespace OpenDMS.Networking.Api.Responses
{
    public abstract class ResponseBase : MessageBase
    {
        public static ResponseBase BuildFrom(Requests.RequestBase request)
        {
            throw new NotImplementedException();
        }

        public static ResponseBase BuildFrom(Requests.RequestBase request, params object[] obj)
        {
            throw new NotImplementedException();
        }
    }
}
