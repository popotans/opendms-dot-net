using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class HeaderCollection 
        : Dictionary<Token, string>
    {
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
    }
}
