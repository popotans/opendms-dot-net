using System;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Networking.Api
{
    public abstract class MessageBase
    {
        protected DateTime _start;
        public JObject FullContent { get; set; }
        public JObject ApplicationContent
        {
            get
            {
                if (FullContent["Application"] != null)
                    return (JObject)FullContent["Application"];
                else
                {
                    FullContent["Application"] = new JObject();
                    return (JObject)FullContent["Application"];
                }
            }
            set
            {
                if (FullContent["Application"] != null)
                    FullContent["Application"] = value;
                else
                    FullContent.Add("Application", value);
            }
        }
        public Type Type
        {
            get
            {
                if (FullContent["Type"] == null)
                    throw new MessageFormattingException("No type specified.");
                return System.Type.GetType(FullContent["Type"].Value<string>());
            }
            set
            {
                if (FullContent["Type"] != null)
                    FullContent["Type"] = value.FullName;
                else
                    FullContent.Add("Type", value.FullName);
            }
        }
        public DateTime Timestamp
        {
            get
            {
                if (FullContent["Timestamp"] == null)
                    throw new MessageFormattingException("No timestamp specified.");
                return FullContent["Timestamp"].Value<DateTime>();
            }
            set
            {
                if (FullContent["Timestamp"] != null)
                    FullContent["Timestamp"] = value;
                else
                    FullContent.Add("Timestamp", value);
            }
        }
        public TimeSpan Duration
        {
            get
            {
                if (FullContent["Duration"] == null)
                    throw new MessageFormattingException("No duration specified.");
                string s = FullContent["Duration"].Value<string>();
                return TimeSpan.Parse(s);
            }
            set
            {
                if (FullContent["Duration"] != null)
                    FullContent["Duration"] = value.ToString();
                else
                    FullContent.Add("Duration", value.ToString());
            }
        }
        public int JsonLength
        {
            get
            {
                if (FullContent["JsonLength"] == null)
                    throw new MessageFormattingException("No json length specified.");
                return int.Parse(FullContent["JsonLength"].Value<string>());
            }
            set
            {
                // 2,147,483,647 <-- max value
                // 1 234 567 89x - 10 characters
                if (FullContent["JsonLength"] != null)
                    FullContent["JsonLength"] = String.Format("{0:0000000000}", value);
                else
                    FullContent.Add("JsonLength", String.Format("{0:0000000000}", value));
            }
        }
        public System.IO.Stream Stream { get; set; }
        //public Http.HttpNetworkStream NetworkStream { get; set; }

        public MessageBase()
        {
            _start = DateTime.Now;
            FullContent = new JObject();
            Type = GetType();
            CalculateJsonLength();
        }

        public MessageBase(JObject fullContent)
        {
            _start = DateTime.Now;
            FullContent = fullContent;
        }

        public int CalculateJsonLength()
        {
            // This could possibly change length as a string, thus we need to either recalculate multiple times until there is
            // no change in length from the last iteration or we need to ensure it takes the same string length no matter the
            // number.  The latter is the best approach as it reduces memory and processor load substantially.
            // Thus, we need to convert to a string and append 0s to the front to fill the full places of a int.

            //System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(FullContent.ToString()));
            //return JsonLength = (int)ms.Length;
            return JsonLength = System.Text.Encoding.UTF8.GetBytes(FullContent.ToString()).Length;
        }

        public MultisourcedStream MakeStream(out long contentLength)
        {
            MultisourcedStream stream;
            Timestamp = DateTime.Now;
            Duration = Timestamp - _start;
            CalculateJsonLength();
            System.IO.MemoryStream msKnot = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonLength.ToString() + "\0"));
            System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(FullContent.ToString()));

            if (Stream != null)
                stream = new MultisourcedStream(msKnot, ms, Stream);
            else
                stream = new MultisourcedStream(msKnot, ms);

            contentLength = stream.Length;
            return stream;
        }
    }
}
