using System;

namespace OpenDMS.Networking.Http
{
    public class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Header(string name, string value)
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
