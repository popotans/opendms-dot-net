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

namespace Common.NetworkPackage
{
    /// <summary>
    /// Represents an individual element of a Dictionary type collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class DictionaryEntry<TKey, TValue> : Base
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TValue Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryEntry&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        public DictionaryEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryEntry&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public DictionaryEntry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Serializes this instance using the specified XML writer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <returns>
        /// The XML writer passed in argument.
        /// </returns>
        public override System.Xml.XmlWriter Serialize(System.Xml.XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Key");
            XmlSerializer.Serialize(xmlWriter, Key);
            xmlWriter.WriteEndElement();
            
            xmlWriter.WriteStartElement("Value");
            if(Value != null)
                XmlSerializer.Serialize(xmlWriter, Value);
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
        public override System.Xml.XmlReader Deserialize(System.Xml.XmlReader xmlReader)
        {
            bool readStart = false;

            if (xmlReader.Name.StartsWith("DictionaryEntry"))
            {
                readStart = true;
                xmlReader.ReadStartElement();
            }
            
            xmlReader.ReadStartElement("Key");
            Key = (TKey)XmlSerializer.SimpleDeserialize(xmlReader);
            xmlReader.ReadEndElement();

            xmlReader.ReadStartElement("Value");
            if (xmlReader.Name == "Key")
                Value = default(TValue);
            else
            {
                Value = (TValue)XmlSerializer.SimpleDeserialize(xmlReader);
                xmlReader.ReadEndElement();
            }

            if (readStart)
                xmlReader.ReadEndElement();

            return xmlReader;
        }
    }
}
