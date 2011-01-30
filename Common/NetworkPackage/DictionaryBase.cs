/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    public class DictionaryBase<TKey, TValue> : Base, 
        System.Collections.Generic.IEnumerable<DictionaryEntry<TKey, TValue>>, 
        System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>
    {
        private int _enumPosition;
        private DictionaryEntry<TKey, TValue>[] _list;
        private int _count;
        private string _rootElementName;

        public int Count { get { return _count; } }
        public string RootElementName { get { return _rootElementName; } }

        public DictionaryBase(string rootElementName)
        {
            _list = new DictionaryEntry<TKey, TValue>[10];
            _rootElementName = rootElementName;
            _enumPosition = -1;
        }

        public override XmlWriter Serialize(XmlWriter xmlWriter)
        {
            if (xmlWriter.Settings.Encoding != Encoding.UTF8)
                throw new Exception("The xml writer's encoding must be UTF8");

            xmlWriter.WriteStartElement(_rootElementName);
            for (int i = 0; i < _count; i++)
                XmlSerializer.Serialize(xmlWriter, _list[i]);
            xmlWriter.WriteEndElement();

            return xmlWriter;
        }

        public override XmlReader Deserialize(XmlReader xmlReader)
        {
            bool loop = true;

            if (xmlReader.IsEmptyElement)
                return xmlReader;

            if (xmlReader.NodeType == XmlNodeType.None)
                xmlReader.MoveToContent();

            while (loop)
            {
                switch (xmlReader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (string.IsNullOrEmpty(xmlReader.LocalName))
                            xmlReader.MoveToContent();
                        if (xmlReader.LocalName == _rootElementName)
                            xmlReader.ReadStartElement(_rootElementName);
                        
                        switch (xmlReader.LocalName)
                        {
                            case "DictionaryEntryOfStringObject":
                                //xmlReader.ReadStartElement("DictionaryEntryOfStringObject");
                                Add(XmlSerializer.Deserialize<DictionaryEntry<string, object>>(new DictionaryEntry<string, object>(), xmlReader));
                                //xmlReader.ReadEndElement();
                                break;
                            case "Key":
                                Add(XmlSerializer.Deserialize<DictionaryEntry<string, object>>(new DictionaryEntry<string, object>(), xmlReader));
                                break;
                            default:
                                throw new NotSupportedException(xmlReader.LocalName + " is not supported.");
                        }

                        if (string.IsNullOrEmpty(xmlReader.LocalName))
                            xmlReader.MoveToContent();
                        if (xmlReader.LocalName == _rootElementName)
                        {
                            xmlReader.ReadEndElement(); // List
                            loop = false;
                        }
                        break;
                    case System.Xml.XmlNodeType.None:
                        loop = false;
                        break;
                    default:
                        xmlReader.MoveToContent();
                        break;
                }
            }

            return xmlReader;
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("Key already exists");

            if (_count + 1 >= _list.Length)
                Expand(10);

            _list[_count++] = new DictionaryEntry<TKey, TValue>(key, value);
        }

        public void Add(object obj)
        {
            if (obj.GetType() == typeof(DictionaryBase<TKey, TValue>))
            {
                DictionaryBase<TKey, TValue> dict = (DictionaryBase<TKey, TValue>)obj;
                for (int i = 0; i < dict.Count; i++)
                    Add(dict[i].Key, dict[i].Value);
            }
            else if (obj.GetType() == typeof(DictionaryEntry<TKey, TValue>))
            {
                DictionaryEntry<TKey, TValue> entry = (DictionaryEntry<TKey, TValue>)obj;
                Add(entry.Key, entry.Value);
            }
            else
                throw new ArgumentException("Argument 'obj' is of unhandled type.");
        }

        public int IndexOf(DictionaryEntry<TKey, TValue> element)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_list[i] == element)
                    return i;
            }

            return -1;
        }

        public int IndexOf(TKey key)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_list[i].Key.Equals(key))
                    return i;
            }

            return -1;
        }

        public void Remove(DictionaryEntry<TKey, TValue> element)
        {
            int index = IndexOf(element);

            if (index >= 0)
                Remove(index);
        }

        public void Remove(int index)
        {
            if (index + 1 > _count)
                throw new IndexOutOfRangeException();

            _list[index] = null;
            for (int i = index; i < _count; i++)
            {
                _list[i] = _list[i + 1];
            }

            _count--;

            if (_count + 10 < _list.Length)
                Constrict(_list.Length - (_count + 10));
        }

        public void Remove(TKey key)
        {
            int index = IndexOf(key);
            if (index < 0)
                throw new ArgumentException("Key does not exist");
            Remove(index);
        }

        public DictionaryEntry<TKey, TValue> this[int index]
        {
            get
            {
                if (index + 1 > _count)
                    throw new IndexOutOfRangeException();

                return _list[index];
            }
            set
            {
                if (index + 1 > _count)
                    throw new IndexOutOfRangeException();

                _list[index] = value;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                for (int i = 0; i < _count; i++)
                    if (_list[i].Key.Equals(key))
                        return _list[i].Value;
                return default(TValue);
            }
            set
            {
                for (int i = 0; i < _count; i++)
                    if (_list[i].Key.Equals(key))
                    {
                        _list[i].Value = value;
                        return;
                    }
                throw new IndexOutOfRangeException();
            }
        }

        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < _count; i++)
                if (_list[i].Key.Equals(key))
                    return true;
            return false;
        }

        private void Expand(int additionalSpace)
        {
            DictionaryEntry<TKey, TValue>[] t = new DictionaryEntry<TKey, TValue>[_list.Length + additionalSpace];
            _list.CopyTo(t, 0);
            _list = t;
        }

        private void Constrict(int removedSpace)
        {
            DictionaryEntry<TKey, TValue>[] t = new DictionaryEntry<TKey, TValue>[_list.Length - removedSpace];
            for (int i = 0; i < _count; i++)
                t[i] = _list[i];
            _list = t;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return (System.Collections.IEnumerator)this;
        }

        public object Current
        {
            get { return _list[_enumPosition]; }
        }

        public bool MoveNext()
        {
            if (_enumPosition < _count - 1)
            {
                _enumPosition++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _enumPosition = -1;
        }

        System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>> System.Collections.Generic.IEnumerable<DictionaryEntry<TKey, TValue>>.GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>)this;
        }

        DictionaryEntry<TKey, TValue> System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>.Current
        {
            get { return _list[_enumPosition]; }
        }

        public void Dispose()
        {
        }
    }
}
