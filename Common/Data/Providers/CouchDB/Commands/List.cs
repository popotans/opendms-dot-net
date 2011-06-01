using System;
using Common.Http.Methods;

namespace Common.Data.Providers.CouchDB.Commands
{
    public class List : Base
    {
        private override HttpGet _httpRequest = null;

        public List(Uri uri)
            : base(new HttpGet(uri))
        {
        }

        public List(HttpRequest get)
            : base(get)
        {
        }
    }
}
