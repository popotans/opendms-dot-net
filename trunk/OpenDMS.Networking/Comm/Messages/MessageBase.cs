using System;
using System.Web;

namespace OpenDMS.Networking.Comm.Messages
{
    public abstract class MessageBase
    {
        public Type Type { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid ConversationId { get; set; }
        public HttpResponse HttpResponse { get; set; }
        public Http.HeadersManager ResponseHeadersManager { get; set; }

        public MessageBase()
        {
            Type = GetType();
            Timestamp = DateTime.Now;
            ResponseHeadersManager = new Http.HeadersManager();
        }

        public MessageBase(HttpResponse response, Guid conversationId)
            : this()
        {
            ConversationId = conversationId;
            ModifyResponse(ResponseHeadersManager, response);
        }

        public MessageBase(HttpRequest request)
        {
            DateTime outTimestamp;
            Guid outGuid;

            string type = request.Headers["X-OpenDMS-Type"];
            string timestamp = request.Headers["X-OpenDMS-Timestamp"];
            string conversationid = request.Headers["X-OpenDMS-ConversationId"];

            if (string.IsNullOrEmpty(type))
                throw new MessageFormattingException("Header type not present.");
            if (string.IsNullOrEmpty(timestamp))
                throw new MessageFormattingException("Header timestamp not present.");
            if (string.IsNullOrEmpty(conversationid))
                throw new MessageFormattingException("Header conversationid not present.");

            try
            {
                Type = System.Type.GetType(type);
            }
            catch (Exception e)
            {
                throw new MessageFormattingException("Invalid type.", e);
            }

            if (!DateTime.TryParse(timestamp, out outTimestamp))
                throw new MessageFormattingException("Invalid timestamp.");

            if (!Guid.TryParse(conversationid, out outGuid))
                throw new MessageFormattingException("Invalid conversation_id.");

            Timestamp = outTimestamp;
            ConversationId = outGuid;
        }

        public abstract System.IO.Stream MakeStream();

        public virtual MessageBase ModifyResponse(Http.HeadersManager headersMgr, HttpResponse response)
        {
            // Since we are not running IIS, but Cassini for a webdev for development in
            // VS - http://stackoverflow.com/questions/2708187/vs2010-development-web-server-does-not-use-integrated-mode-http-handlers-modules

            if (headersMgr["X-OpenDMS-Type"] == null)
                headersMgr.AddHeader("X-OpenDMS-Type", Type.FullName);
            else
                headersMgr["X-OpenDMS-Type"].Value = Type.FullName;

            if (headersMgr["X-OpenDMS-Timestamp"] == null)
                headersMgr.AddHeader("X-OpenDMS-Timestamp", Timestamp.ToString());
            else
                headersMgr["X-OpenDMS-Timestamp"].Value = Timestamp.ToString();

            if (headersMgr["X-OpenDMS-ConversationId"] == null)
                headersMgr.AddHeader("X-OpenDMS-ConversationId", ConversationId.ToString());
            else
                headersMgr["X-OpenDMS-ConversationId"].Value = ConversationId.ToString();

            headersMgr.AttachHeadersToResponse(response, true);

            return this;
        }
    }
}
