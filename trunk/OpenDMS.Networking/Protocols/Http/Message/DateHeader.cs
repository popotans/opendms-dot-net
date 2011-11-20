using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class DateHeader : Header
    {
        public static new string NAME { get { return "Date"; } }

        public DateHeader(DateTime value)
            : base(new Token(NAME), value.ToString("r"))
        {
        }

        public DateHeader(string value)
            : base(new Token(NAME), DateTime.Parse(value).ToString("r")) // Parse and back to ensure proper formatting
        {
        }
    }
}
