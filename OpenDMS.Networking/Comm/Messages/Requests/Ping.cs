using System;
using System.Web;

namespace OpenDMS.Networking.Comm.Messages.Requests
{
    public class Ping : JsonOnlyBase, IRequest
    {
        public Ping()
            : base()
        {
        }

        public Ping(HttpRequest request)
            : base(request)
        {
        }

        public Responses.IResponse BuildResponseMessage(HttpResponse response)
        {
            return new Responses.Pong(response, ConversationId);
        }
    }
}
