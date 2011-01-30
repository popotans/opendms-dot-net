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
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Common
{
    public class SerializableStringList : List<string>
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool loop = true;
            //XmlSerializer valueSerializer = new XmlSerializer(typeof(string));

            if (reader.IsEmptyElement)
                return;

            while (loop)
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (reader.LocalName == "List")
                            reader.ReadStartElement("List");
                        string value = reader.GetAttribute("value");
                        reader.ReadStartElement("item");
                        //string value = reader.GetAttribute("value");
                        this.Add(value);
                        if (string.IsNullOrEmpty(reader.LocalName))
                            reader.MoveToContent();
                        if (reader.LocalName == "List")
                        {
                            reader.ReadEndElement(); // List
                            loop = false;
                        }
                        break;
                    case System.Xml.XmlNodeType.None:
                        loop = false;
                        break;
                    default:
                        reader.MoveToContent();
                        break;
                }
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("List");
            for (int i=0; i < Count; i++)
            {
                writer.WriteStartElement("item");
                writer.WriteStartAttribute("value");
                writer.WriteString(this[i]);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public SerializableStringList Import(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
                Add(list[i]);

            return this;
        }

        public List<string> ToList()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < Count; i++)
                list.Add(this[i]);
            return list;
        }
    }
}
