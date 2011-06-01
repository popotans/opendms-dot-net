using System;
using Common.Http.Methods;

namespace Common.Data.Providers.CouchDB.Commands
{
    public class Get : Base
    {
        private override HttpGet _httpRequest = null;

        public Get(Uri uri)
            : base(new HttpGet(uri))
        {
        }

        public Get(HttpRequest get)
            : base(get)
        {
        }
    }
}
