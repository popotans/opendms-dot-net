using System;
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
            if (key.StartsWith("$"))
                throw new ArgumentException("The key cannot begin with the '$' character.");
            _properties.Add(key, value);
        }

        public void Remove(string key)
        {
            if (key.StartsWith("$"))
                throw new ArgumentException("The key cannot begin with the '$' character.");
            _properties.Remove(key);
        }

        public object this[string key] 
        {
            get
            {
                if (key.StartsWith("$"))
                    throw new ArgumentException("Properties begining with the '$' character cannot be accessed through the Metadata class.");
                return _properties[key]; 
            }
            set
            {
                if (key.StartsWith("$"))
                    throw new ArgumentException("The key cannot begin with the '$' character.");
                _properties[key] = value; 
            }
        }

        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}
