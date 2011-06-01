using System;
using Common.Http.Methods;

namespace Common.Data.Providers.CouchDB.Commands
{
    public class Update : Base
    {
        private override HttpPut _httpRequest = null;
        
        public Update(Uri uri)
            : base(new HttpPut(uri, "application/json"))
        {
        }

        public Update(HttpRequest put)
            : base(put)
        {
        }
    }
}
