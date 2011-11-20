using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class ContentMd5Header : Header
    {
        public static new string NAME { get { return "Content-MD5"; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">base64 of 128 bit MD5 digest as per RFC 1864</param>
        public ContentMd5Header(string value)
            : base(new Token(NAME), value)
        {
        }
    }
}
