using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class View : BaseStorageObject
    {
        public View()
        {
        }

        public View(string json)
            : this(Parse(json))
        {
        }

        public View(JToken token)
            : this((JObject)token)
        {
        }

        public View(JObject jobj)
            : base(jobj)
        {
        }
    }
}
