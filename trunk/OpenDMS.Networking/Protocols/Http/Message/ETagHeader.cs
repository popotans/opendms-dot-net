using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class ETagHeader : Header
    {
        public static override string NAME { get { return "ETag"; } }

        public ETagHeader(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
