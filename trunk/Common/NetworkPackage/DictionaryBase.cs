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
    /// <summary>
    /// Represents a serializable Dictionary's base requirements for inheriting classes.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class DictionaryBase<TKey, TValue> : Base, 
        System.Collections.Generic.IEnumerable<DictionaryEntry<TKey, TValue>>, 
        System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>
    {
        /// <summary>
        /// A positional context for enumeration.
        /// </summary>
        private int _enumPosition;
        /// <summary>
        /// Any array of DictionaryEntry objects.
        /// </summary>
        private DictionaryEntry<TKey, TValue>[] _list;
        /// <summary>
        /// The quantity of elements in this collection.
        /// </summary>
        private int _count;
        /// <summary>
        /// The name of the root element
        /// </summary>
        private string _rootElementName;

        /// <summary>
        /// Gets the quantity of elements in this collection.
        /// </summary>
        public int Count { get { return _count; } }
        /// <summary>
        /// Gets the name of the root element.
        /// </summary>
        /// <value>
        /// The name of the root element.
        /// </value>
        public string RootElementName { get { return _rootElementName; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryBase&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="rootElementName">Name of the root element.</param>
        public DictionaryBase(string rootElementName)
        {
            _list = new DictionaryEntry<TKey, TValue>[10];
            _rootElementName = rootElementName;
            _enumPosition = -1;
        }

        /// <summary>
        /// Serializes this instance using the specified XML writer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <returns>
        /// The XML writer passed in argument.
        /// </returns>
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

        /// <summary>
        /// Deserializes the content of the specified XML reader populating the properties of this instance.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <returns>
        /// The XML reader passed in argument.
        /// </returns>
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

        /// <summary>
        /// Adds a DictionaryEntry with the specified key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("Key already exists");

            if (_count + 1 >= _list.Length)
                Expand(10);

            _list[_count++] = new DictionaryEntry<TKey, TValue>(key, value);
        }

        /// <summary>
        /// Adds a DictionaryEntry if the type can be converted to a useable type.
        /// </summary>
        /// <param name="obj">The obj.</param>
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
                if (!ContainsKey(entry.Key))
                    Add(entry.Key, entry.Value);
                else
                    this[entry.Key] = entry.Value;
            }
            else
                throw new ArgumentException("Argument 'obj' is of unhandled type.");
        }

        /// <summary>
        /// Find the index of the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A zero based index of the position in the collection.</returns>
        public int IndexOf(DictionaryEntry<TKey, TValue> element)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_list[i] == element)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A zero based index of the position in the collection.</returns>
        public int IndexOf(TKey key)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_list[i].Key.Equals(key))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Removes the specified element from the collection.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Remove(DictionaryEntry<TKey, TValue> element)
        {
            int index = IndexOf(element);

            if (index >= 0)
                Remove(index);
        }

        /// <summary>
        /// Removes the element from the collection at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
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

        /// <summary>
        /// Removes the element from the collection where the key matches.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            int index = IndexOf(key);
            if (index < 0)
                throw new ArgumentException("Key does not exist");
            Remove(index);
        }

        /// <summary>
        /// Gets or sets the <see cref="Common.NetworkPackage.DictionaryEntry&lt;TKey,TValue&gt;"/> at the specified index.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the <typeparamref name="TValue"/> with the specified key.
        /// </summary>
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

        /// <summary>
        /// Determines whether the collection contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < _count; i++)
                if (_list[i].Key.Equals(key))
                    return true;
            return false;
        }

        /// <summary>
        /// Expands the collection's maximum size by the specified additional space.
        /// </summary>
        /// <param name="additionalSpace">The additional space.</param>
        private void Expand(int additionalSpace)
        {
            DictionaryEntry<TKey, TValue>[] t = new DictionaryEntry<TKey, TValue>[_list.Length + additionalSpace];
            _list.CopyTo(t, 0);
            _list = t;
        }

        /// <summary>
        /// Constricts the collection's maximum size by the specified removed space.
        /// </summary>
        /// <param name="removedSpace">The removed space.</param>
        private void Constrict(int removedSpace)
        {
            DictionaryEntry<TKey, TValue>[] t = new DictionaryEntry<TKey, TValue>[_list.Length - removedSpace];
            for (int i = 0; i < _count; i++)
                t[i] = _list[i];
            _list = t;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return (System.Collections.IEnumerator)this;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        /// The element in the collection at the current position of the enumerator.
        ///   </returns>
        public object Current
        {
            get { return _list[_enumPosition]; }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        ///   </exception>
        public bool MoveNext()
        {
            if (_enumPosition < _count - 1)
            {
                _enumPosition++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">
        /// The collection was modified after the enumerator was created.
        ///   </exception>
        public void Reset()
        {
            _enumPosition = -1;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>> System.Collections.Generic.IEnumerable<DictionaryEntry<TKey, TValue>>.GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>)this;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        /// The element in the collection at the current position of the enumerator.
        ///   </returns>
        DictionaryEntry<TKey, TValue> System.Collections.Generic.IEnumerator<DictionaryEntry<TKey, TValue>>.Current
        {
            get { return _list[_enumPosition]; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
