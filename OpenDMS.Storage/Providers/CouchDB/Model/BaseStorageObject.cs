using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public abstract class BaseStorageObject : JObject
    {
        private ulong _length = 0;

        public ulong Length
        {
            get
            {
                if (_length == 0)
                    return (_length = (ulong)this.ToString().Length);
                return _length;
            }
        }

        public BaseStorageObject()
        {
            _length = 0;
        }

        public BaseStorageObject(string json)
            : this(Parse(json))
        {
        }

        public BaseStorageObject(JToken token)
            : this((JObject)token)
        {
        }

        public BaseStorageObject(JObject jobj)
            : base(jobj)
        {
        }

        protected void ResetLength()
        {
            _length = 0;
        }
    }
}