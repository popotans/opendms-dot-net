using System; 
using System.Collections.Generic; 
using System.Text; 
using System.Xml.Serialization;

namespace Common
{
    //public class SerializableDictionary : Dictionary<string, object>, IXmlSerializable
    //{
    //    public System.Xml.Schema.XmlSchema GetSchema()
    //    {
    //        return null;
    //    }

    //    public void ReadXml(System.Xml.XmlReader reader)
    //    {
    //        bool loop = true;
    //        XmlSerializer keySerializer = new XmlSerializer(typeof(string));
    //        XmlSerializer valueSerializer;

    //        if(reader.IsEmptyElement)
    //            return;

    //        while (loop)
    //        {
    //            switch (reader.NodeType)
    //            {
    //                case System.Xml.XmlNodeType.Element:
    //                    if (reader.LocalName == "Dictionary")
    //                        reader.ReadStartElement("Dictionary");
    //                    reader.ReadStartElement("item");
    //                    reader.ReadStartElement("key");
    //                    string key = (string)keySerializer.Deserialize(reader);
    //                    reader.ReadEndElement();
    //                    reader.ReadStartElement("value");
    //                    if (string.IsNullOrEmpty(reader.LocalName))
    //                        reader.MoveToContent();
    //                    switch(reader.LocalName)
    //                    {
    //                        case "List":                        
    //                            SerializableStringList list = new SerializableStringList();
    //                            list.ReadXml(reader);
    //                            this.Add(key, list);
    //                            break;
    //                        case "string":
    //                            valueSerializer = new XmlSerializer(typeof(string));
    //                            this.Add(key, (string)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "int":
    //                            valueSerializer = new XmlSerializer(typeof(int));
    //                            this.Add(key, (int)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "unsignedInt":
    //                            valueSerializer = new XmlSerializer(typeof(uint));
    //                            this.Add(key, (uint)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "long":
    //                            valueSerializer = new XmlSerializer(typeof(long));
    //                            this.Add(key, (long)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "unsignedLong":
    //                            valueSerializer = new XmlSerializer(typeof(ulong));
    //                            this.Add(key, (ulong)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "dateTime":
    //                            valueSerializer = new XmlSerializer(typeof(DateTime));
    //                            this.Add(key, (DateTime)valueSerializer.Deserialize(reader));
    //                            break;
    //                        case "FormItem":
    //                            valueSerializer = new XmlSerializer(typeof(NetworkPackage.FormProperty));
    //                            this.Add(key, (NetworkPackage.FormProperty)valueSerializer.Deserialize(reader));
    //                            //Network.FormProperty fi = new Network.FormProperty();
    //                            //fi.ReadXml(reader);
    //                            //this.Add(key, fi);
    //                            break;
    //                        default:
    //                            throw new Exception("Unsupported data type: " + reader.LocalName);
    //                    }
    //                    reader.ReadEndElement();
    //                    reader.ReadEndElement(); // item
    //                    reader.MoveToContent();
    //                    if (reader.LocalName == "Dictionary")
    //                    {
    //                        reader.ReadEndElement(); // Dictionary
    //                        loop = false;
    //                    }
    //                    break;
    //                case System.Xml.XmlNodeType.None:
    //                    loop = false;
    //                    break;
    //                default:
    //                    reader.MoveToContent();
    //                    break;
    //            }
    //        }
    //    }

    //    public void WriteXml(System.Xml.XmlWriter writer)
    //    {
    //        XmlSerializer keySerializer = new XmlSerializer(typeof(string));
    //        //XmlSerializer valueSerializer = new XmlSerializer(typeof(object));

    //        writer.WriteStartElement("Dictionary");
    //        foreach (string key in this.Keys)
    //        {
    //            writer.WriteStartElement("item");

    //            writer.WriteStartElement("key");
    //            keySerializer.Serialize(writer, key);
    //            writer.WriteEndElement();

    //            writer.WriteStartElement("value");
    //            object value = this[key];
                
    //            if (value.GetType() == typeof(List<string>))
    //            {
    //                SerializableStringList list = new SerializableStringList();
    //                list.Import((List<string>)value);
    //                list.WriteXml(writer);
    //            }
    //            //else if (value.GetType() == typeof(NetworkPackage.FormProperty))
    //            //{
    //            //    XmlSerializer ser = new XmlSerializer(value.GetType());
    //            //    ser.Serialize(writer, value);
    //            //    NetworkPackage.FormProperty fp = (NetworkPackage.FormProperty)value;
    //            //    fp.WriteXml(writer);
    //            //}
    //            else
    //            {
    //                XmlSerializer ser = new XmlSerializer(value.GetType());
    //                ser.Serialize(writer, value);
    //            }

    //            writer.WriteEndElement();

    //            writer.WriteEndElement();
    //        }
    //        writer.WriteEndElement();
    //    }

    //    public void Import(Dictionary<string, object> dict)
    //    {
    //        Dictionary<string, object>.Enumerator en = dict.GetEnumerator();
    //        while (en.MoveNext())
    //        {
    //            Add(en.Current.Key, en.Current.Value);
    //        }
    //    }
    //}
}
