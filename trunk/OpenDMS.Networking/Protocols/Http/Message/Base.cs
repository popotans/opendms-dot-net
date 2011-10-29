using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public abstract class Base
    {
        public HeaderCollection Headers { get; set; }
        public Body Body { get; set; }

        public long? ContentLength
        {
            get
            {
                KeyValuePair<Token, string>? kvp = Headers.GetEntry(ContentLengthHeader.NAME);
                if (kvp.HasValue) return long.Parse(kvp.Value.Value);
                else return null;
            }
            set
            {
                Headers[ContentLengthHeader.NAME] = value.ToString();
            }
        }

        public string ContentType
        {
            get
            {
                KeyValuePair<Token, string>? kvp = Headers.GetEntry(ContentTypeHeader.NAME);
                if (kvp.HasValue) return kvp.Value.Value;
                else return null;
            }
            set
            {
                Headers[ContentTypeHeader.NAME] = value;
            }
        }
        
        public Base()
        {
            Headers = new HeaderCollection();
            Body = new Body();
        }
    }
}
