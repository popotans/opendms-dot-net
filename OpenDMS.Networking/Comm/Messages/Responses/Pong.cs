using System;
using System.Web;

namespace OpenDMS.Networking.Comm.Messages.Responses
{
    public class Pong : JsonOnlyBase, IResponse
    {
        public Pong()
            : base()
        {
        }

        public Pong(HttpResponse response, Guid conversationId)
            : base(response, conversationId)
        {
        }
    }
}
