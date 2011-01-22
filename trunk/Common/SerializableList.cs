using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Common
{
    public class SerializableList<T> : List<T>
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool loop = true;
            XmlSerializer valueSerializer = new XmlSerializer(typeof(T));

            if (reader.IsEmptyElement)
                return;

            while (loop)
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (reader.LocalName == "List")
                            reader.ReadStartElement("List");
                        reader.ReadStartElement("item");
                        this.Add((T)valueSerializer.Deserialize(reader));
                        reader.ReadEndElement();
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
            XmlSerializer valueSerializer = new XmlSerializer(typeof(T));

            writer.WriteStartElement("List");
            for (int i = 0; i < Count; i++)
            {
                writer.WriteStartElement("item");
                valueSerializer.Serialize(writer, this[i]);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void Import(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                Add(list[i]);
        }

        public List<T> ToList()
        {
            List<T> list = new List<T>();
            for (int i = 0; i < Count; i++)
                list.Add(this[i]);
            return list;
        }
    }
}