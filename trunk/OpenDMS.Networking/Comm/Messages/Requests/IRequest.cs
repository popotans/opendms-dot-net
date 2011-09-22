using System;
using System.Web;

namespace OpenDMS.Networking.Comm.Messages.Requests
{
    public interface IRequest
    {
        Responses.IResponse BuildResponseMessage(HttpResponse response);
    }
}
