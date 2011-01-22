using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    public class FormProperty : Base
    {
        public enum SupportedDataType
        {
            Text = 0,
            Date,
            TextCollection
        }

        public SupportedDataType DataType;
        public string Label;
        public string PropertyName;
        public string DefaultValue;

        public FormProperty()
        {
        }

        public FormProperty(SupportedDataType controlType, string label, string propertyName, string defaultValue)
        {
            DataType = controlType;
            Label = label;
            PropertyName = propertyName;
            DefaultValue = defaultValue;
        }

        public override XmlWriter Serialize(XmlWriter xmlWriter)
        {
            if (xmlWriter.Settings.Encoding != Encoding.UTF8)
                throw new Exception("The xml writer's encoding must be UTF8");

            xmlWriter.WriteStartElement("FormProperty");
            xmlWriter.WriteAttributeString("DataType", DataType.ToString());
            xmlWriter.WriteAttributeString("Label", Label);
            xmlWriter.WriteAttributeString("PropertyName", PropertyName);
            xmlWriter.WriteAttributeString("DefaultValue", DefaultValue);
            xmlWriter.WriteEndElement();

            return xmlWriter;
        }

        public override XmlReader Deserialize(XmlReader xmlReader)
        {
            // Move to content
            xmlReader.MoveToContent();
            if (xmlReader.Name != "FormProperty")
                throw new FormatException("Invalid XML expected FormProperty");

            DataType = (SupportedDataType)Enum.Parse(typeof(SupportedDataType), xmlReader.GetAttribute("DataType"));
            Label = xmlReader.GetAttribute("Label");
            PropertyName = xmlReader.GetAttribute("PropertyName");
            DefaultValue = xmlReader.GetAttribute("DefaultValue");

            return xmlReader;
        }
    }
}
