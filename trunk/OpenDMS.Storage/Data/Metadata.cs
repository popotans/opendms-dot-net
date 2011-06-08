using System.Collections.Generic;

namespace OpenDMS.Storage.Data
{
    public class Metadata
    {
        private Dictionary<string, object> _properties { get; set; }

        public Metadata()
        {
            _properties = new Dictionary<string, object>();
        }

        public void Add(string key, object value)
        {
            _properties.Add(key, value);
        }

        public void Remove(string key)
        {
            _properties.Remove(key);
        }

        public object this[string key] 
        {
            get { return _properties[key]; }
            set { _properties[key] = value; }
        }

        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}
