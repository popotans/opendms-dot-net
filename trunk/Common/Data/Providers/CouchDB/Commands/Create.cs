using System;
using Common.Http.Methods;

namespace Common.Data.Providers.CouchDB.Commands
{
    public class Create : Base
    {
        private override HttpPut _httpRequest = null;

        public Create(Uri uri)
            : base(new HttpPut(uri, "application/json"))
        {
        }

        public Create(HttpRequest put)
            : base(put)
        {
        }
    }
}
