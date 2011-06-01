using System;
using Common.Http.Methods;

namespace Common.Data.Providers.CouchDB.Commands
{
    public class Delete : Base
    {
        private override HttpDelete _httpRequest = null;

        public Delete(Uri uri)
            : base(new HttpDelete(uri))
        {
        }

        public Delete(HttpRequest delete)
            : base(delete)
        {
        }
    }
}
