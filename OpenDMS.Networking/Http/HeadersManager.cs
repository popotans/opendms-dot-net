using System;
using System.Web;
using System.Collections.Generic;

namespace OpenDMS.Networking.Http
{
    public class HeadersManager
    {
        private Dictionary<string, Header> _headers;

        public HeadersManager()
        {
            _headers = new Dictionary<string, Header>();
        }

        public Header this[string key]
        {
            get
            {
                if (_headers.ContainsKey(key))
                    return _headers[key];
                return null;
            }
            set
            {
                if (_headers.ContainsKey(key))
                    _headers[key] = value;
                else
                    _headers.Add(key, value);
            }
        }

        public void AddHeader(Header header)
        {
            _headers.Add(header.Name, header);
        }

        public void AddHeader(string name, string value)
        {
            _headers.Add(name, new Header(name, value));
        }

        public void RemoveHeader(string key)
        {
            _headers.Remove(key);
        }

        public void RemoveHeader(Header header)
        {
            RemoveHeader(header.Name);
        }

        public void AttachHeadersToResponse(HttpResponse response, bool clearExistingHeaders)
        {
            Dictionary<string, Header>.Enumerator en = _headers.GetEnumerator();

            if(clearExistingHeaders) response.ClearHeaders();

            while (en.MoveNext())
            {
                response.AddHeader(en.Current.Value.Name, en.Current.Value.Value);
            }
        }
    }
}
