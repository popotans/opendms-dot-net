using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class HeaderCollection 
        : Dictionary<Token, string>
    {
        public HeaderCollection()
            : base()
        {
        }

        public HeaderCollection(System.Net.WebHeaderCollection headers)
            : base()
        {
            for (int i = 0; i < headers.Count; i++)
            {
                Add(new Header(new Token(headers.Keys[i]), headers[i]));
            }
        }

        public HeaderCollection(System.Collections.Specialized.NameValueCollection headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                Add(new Header(new Token(headers.Keys[i]), headers[i]));
            }
        }

        public void Add(Header header)
        {
            Add(header.Name, header.Value);
        }

        public bool ContainsKey(Header header)
        {
            return base.ContainsKey(header.Name);
        }

        public bool ContainsKey(string key)
        {
            return ContainsKey(new Token(key));
        }

        public KeyValuePair<Token, string>? GetEntry(string key)
        {
            Dictionary<Token, string>.Enumerator en;
            KeyValuePair<Token, string> kvp;

            en = GetEnumerator();

            while (en.MoveNext())
            {
                if (en.Current.Key.Value == key)
                    return en.Current;
            }                   
 
            return null;
        }

        public string this[string key]
        {
            get
            {
                return this[new Token(key)];
            }
            set
            {
                KeyValuePair<Token, string>? kvp = GetEntry(key);
                if (kvp.HasValue) this[kvp.Value.Key] = value;
                else Add(new Token(key), value);
            }
        }

        public override string ToString()
        {
            Dictionary<Token, string>.Enumerator en;
            string output = "";

            en = GetEnumerator();

            while (en.MoveNext())
            {
                output += en.Current.Key.Value + ": " + en.Current.Value + "\r\n";
            }

            // trim off the last \r\n
            return output.Substring(0, output.Length - 2);
        }
    }
}
