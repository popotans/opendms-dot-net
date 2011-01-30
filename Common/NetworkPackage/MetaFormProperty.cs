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
using System.Collections.Generic;

namespace Common.NetworkPackage
{
    public class MetaFormProperty : Base
    {
        private List<Type> _supportedDataTypes;

        private object _defaultValue;

        /// <summary>
        /// The Type of this property
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// The name of the field in the Data.MetaAsset class
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The text value to display on the UI
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The value of the property if no other value has been specified
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                if (value == null)
                    _defaultValue = value;
                else if (SupportedTypeExists(value.GetType()))
                    _defaultValue = value;
                else
                {
                    Type type = value.GetType();
                    throw new ArgumentException("The Type '" + type.FullName + "' is currently unsupported.");
                }
            }
        }

        public bool IsReadOnly { get; set; }

        public MetaFormProperty()
        {
            _supportedDataTypes = new List<Type>();
            _supportedDataTypes.Add(typeof(string));
            _supportedDataTypes.Add(typeof(Guid));
            _supportedDataTypes.Add(typeof(Int32));
            _supportedDataTypes.Add(typeof(UInt32));
            _supportedDataTypes.Add(typeof(Int64));
            _supportedDataTypes.Add(typeof(UInt64));
            _supportedDataTypes.Add(typeof(DateTime));
            _supportedDataTypes.Add(typeof(System.Collections.Generic.List<string>));
            //_supportedDataTypes.Add(typeof(Dictionary<string, object>));
            _supportedDataTypes.Add(typeof(Common.Data.ETag));

            DataType = null;
            PropertyName = null;
            Label = null;
            DefaultValue = null;
            IsReadOnly = true;
        }

        public MetaFormProperty(Type type, string propertyName, string label, object defaultValue, bool isReadOnly) :
            this()
        {
            DataType = type;
            PropertyName = propertyName;
            Label = label;
            DefaultValue = defaultValue;
            IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Determines if the specified type is supported
        /// </summary>
        /// <param name="type">Type to test</param>
        /// <returns>True if this type is supported, false otherwise</returns>
        private bool SupportedTypeExists(Type type)
        {
            for (int i = 0; i < _supportedDataTypes.Count; i++)
            {
                if (_supportedDataTypes[i] == type)
                    return true;
            }

            return false;
        }

        public override XmlWriter Serialize(XmlWriter xmlWriter)
        {
            if (xmlWriter.Settings.Encoding != Encoding.UTF8)
                throw new Exception("The xml writer's encoding must be UTF8");

            xmlWriter.WriteStartElement("MetaFormProperty");
            xmlWriter.WriteAttributeString("DataType", DataType.FullName);
            xmlWriter.WriteAttributeString("PropertyName", PropertyName);
            xmlWriter.WriteAttributeString("Label", Label);
            xmlWriter.WriteAttributeString("IsReadOnly", IsReadOnly.ToString());

            if (DefaultValue == null)
                xmlWriter.WriteAttributeString("DefaultValue", null);
            else
                xmlWriter.WriteAttributeString("DefaultValue", DefaultValue.ToString());

            xmlWriter.WriteEndElement();

            return xmlWriter;
        }

        public override XmlReader Deserialize(XmlReader xmlReader)
        {
            // Move to content
            xmlReader.MoveToContent();
            if (xmlReader.Name != "MetaFormProperty")
                throw new FormatException("Invalid XML expected MetaFormProperty");
            
            DataType = Type.GetType(xmlReader.GetAttribute("DataType"));
            Label = xmlReader.GetAttribute("Label");
            PropertyName = xmlReader.GetAttribute("PropertyName");
            IsReadOnly = bool.Parse(xmlReader.GetAttribute("IsReadOnly"));

            switch (DataType.FullName)
            {
                case "System.String":
                    _defaultValue = xmlReader.GetAttribute("DefaultValue");
                    break;
                case "System.Guid":
                    _defaultValue = new Guid(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.Int32":
                    _defaultValue = Int32.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.UInt32":
                    _defaultValue = UInt32.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.Int64":
                    _defaultValue = Int64.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.UInt64":
                    _defaultValue = UInt64.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.DateTime":
                    _defaultValue = DateTime.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.Bool":
                    _defaultValue = bool.Parse(xmlReader.GetAttribute("DefaultValue"));
                    break;
                case "System.Collections.Generic.List`1[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                    _defaultValue = new System.Collections.Generic.List<string>();
                    break;
                case "System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                    _defaultValue = new System.Collections.Generic.List<string>();
                    break;
                case "Common.Data.ETag":
                    _defaultValue = new Common.Data.ETag(xmlReader.GetAttribute("DefaultValue"));
                    break;
                default:
                    throw new Exception("Unsupported type detected '" + DataType.GetType().FullName + "'.");
            }

            return xmlReader;
        }
    }
}
