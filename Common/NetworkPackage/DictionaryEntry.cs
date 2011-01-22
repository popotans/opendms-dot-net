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
