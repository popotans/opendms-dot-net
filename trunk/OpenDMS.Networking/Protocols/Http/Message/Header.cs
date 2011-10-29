using System;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class Header
    {
        public abstract static string NAME { get; }

        public Token Name { get; set; }
        public string Value { get; set; }

        public Header(Token name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
