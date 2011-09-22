using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Comm.Messages
{
    public class JsonOnlyBase : MessageBase
    {
        public JObject JObject { get; set; }

        public JsonOnlyBase()
            : base()
        {
        }

        public JsonOnlyBase(HttpResponse response, Guid conversationId)
            : base(response, conversationId)
        {
        }

        public JsonOnlyBase(HttpRequest request)
            : base(request)
        {
            int bytesRead = 0;
            string str = "";
            byte[] buffer = new byte[8192];
            int bytesToRead = 8192;

            if (request.ContentLength == 0)
            {
                JObject = null;
                return;
            }

            if (request.ContentLength < bytesToRead)
                bytesToRead = request.ContentLength;

            while ((bytesRead = request.InputStream.Read(buffer, 0, bytesToRead)) > 0)
            {
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if ((bytesToRead -= bytesRead) <= 0)
                    break;
            }

            JObject = JObject.Parse(str);
        }

        public override System.IO.Stream MakeStream()
        {
            return new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(JObject.ToString()));
        }
    }
}
