using System;
using System.IO;
using System.Reflection;

namespace Common.NetworkPackage
{
    public class XmlSerializer
    {
        public static string SerializeToString(object obj)
        {
            MemoryStream ms = SerializeToStream(obj);
            StreamReader sr = new StreamReader(ms);
            string s = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            ms.Close();
            return s;
        }

        public static System.Xml.XmlWriter Serialize(System.Xml.XmlWriter xmlWriter, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            string str;

            if (xmlWriter.Settings.Encoding != System.Text.Encoding.UTF8)
                throw new FormatException("Encoding must be UTF8");

            if (obj.GetType() == typeof(DictionaryBase<string, object>))
            {
                xmlWriter = ((DictionaryBase<string, object>)obj).Serialize(xmlWriter);
            }
            else if (obj.GetType() == typeof(DictionaryEntry<string, object>))
            {
                xmlWriter = ((DictionaryEntry<string, object>)obj).Serialize(xmlWriter);
            }
            else if (obj.GetType() == typeof(FormProperty))
            {
                xmlWriter = ((FormProperty)obj).Serialize(xmlWriter);
            }
            else if (obj.GetType() == typeof(MetaAsset))
            {
                xmlWriter = ((MetaAsset)obj).Serialize(xmlWriter);
            }
            else
            {
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                ser.Serialize(xmlWriter, obj);
                MemoryStream ms = new MemoryStream();
                ser.Serialize(ms, obj);
                str = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlWriter;
        }

        public static MemoryStream SerializeToStream(object obj)
        {
            MemoryStream ms = new MemoryStream();
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(ms, 
                new System.Xml.XmlWriterSettings() { Encoding = System.Text.Encoding.UTF8 });
            Serialize(writer, obj);
            ms.Position = 0;
            writer.Close();
            return ms;
        }

        public static T Deserialize<T>(object target, string str)
        {
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(
                new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str)), 
                new System.Xml.XmlReaderSettings() { IgnoreWhitespace = true });
            target = (T)Deserialize<T>(target, xmlReader);
            xmlReader.Close();
            return (T)target;
        }

        public static T Deserialize<T>(object target, System.Xml.XmlReader xmlReader)
        {
            if (target.GetType() == typeof(DictionaryEntry<string, object>))
            {
                if(target == null) target = new DictionaryEntry<string, object>();
                xmlReader = ((DictionaryEntry<string, object>)target).Deserialize(xmlReader);
            }
            else if (target.GetType() == typeof(MetaAsset))
            {
                if (target == null) target = new MetaAsset();
                xmlReader = ((MetaAsset)target).Deserialize(xmlReader);
            }
            else
            {
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(target.GetType());
                try
                {
                    target = (T)ser.Deserialize(xmlReader);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return (T)target;
        }

        public static object SimpleDeserialize(System.Xml.XmlReader xmlReader)
        {
            System.Xml.Serialization.XmlSerializer valueSerializer = null;

            switch (xmlReader.LocalName)
            {
                case "boolean":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(bool));
                    return valueSerializer.Deserialize(xmlReader);
                case "string":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(string));
                    return valueSerializer.Deserialize(xmlReader);
                case "guid":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Guid));
                    return valueSerializer.Deserialize(xmlReader);
                case "object":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(object));
                    return valueSerializer.Deserialize(xmlReader);
                case "dateTime":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(DateTime));
                    return valueSerializer.Deserialize(xmlReader);
                case "int":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(int));
                    return valueSerializer.Deserialize(xmlReader);
                case "unsignedInt":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(uint));
                    return valueSerializer.Deserialize(xmlReader);
                case "long":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(long));
                    return valueSerializer.Deserialize(xmlReader);
                case "unsignedLong":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(ulong));
                    return valueSerializer.Deserialize(xmlReader);
                case "double":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(double));
                    return valueSerializer.Deserialize(xmlReader);
                case "ListOfString":
                    return XmlSerializer.Deserialize<List<string>>(new List<string>(), xmlReader);
                case "ArrayOfString":
                    string[] a = new string[1];
                    return XmlSerializer.Deserialize<string[]>(a, xmlReader);
                case "ErrorCode":
                    valueSerializer = new System.Xml.Serialization.XmlSerializer(typeof(NetworkPackage.ServerResponse.ErrorCode));
                    return valueSerializer.Deserialize(xmlReader);
                default:
                    throw new NotSupportedException();
            }

        }
    }
}