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
    public class DictionaryEntry<TKey, TValue> : Base
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public DictionaryEntry()
        {
        }

        public DictionaryEntry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

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
