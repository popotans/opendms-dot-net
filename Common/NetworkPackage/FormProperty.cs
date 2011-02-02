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
    /// Provides a method of packaging and unpackaging a description of a property to display to a 
    /// user on a UI search form.
    /// </summary>
    public class FormProperty : Base
    {
        /// <summary>
        /// An enumeration of supported data types.
        /// </summary>
        public enum SupportedDataType
        {
            /// <summary>
            /// Text
            /// </summary>
            Text = 0,
            /// <summary>
            /// A date
            /// </summary>
            Date,
            /// <summary>
            /// A collection of text
            /// </summary>
            TextCollection
        }

        /// <summary>
        /// The data type of this instance.
        /// </summary>
        public SupportedDataType DataType;
        /// <summary>
        /// The label to display to the user.
        /// </summary>
        public string Label;
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName;
        /// <summary>
        /// The default value of the property.
        /// </summary>
        public string DefaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormProperty"/> class.
        /// </summary>
        public FormProperty()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormProperty"/> class.
        /// </summary>
        /// <param name="controlType">The data type of this instance</param>
        /// <param name="label">The label to display to the user.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        public FormProperty(SupportedDataType controlType, string label, string propertyName, string defaultValue)
        {
            DataType = controlType;
            Label = label;
            PropertyName = propertyName;
            DefaultValue = defaultValue;
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

            xmlWriter.WriteStartElement("FormProperty");
            xmlWriter.WriteAttributeString("DataType", DataType.ToString());
            xmlWriter.WriteAttributeString("Label", Label);
            xmlWriter.WriteAttributeString("PropertyName", PropertyName);
            xmlWriter.WriteAttributeString("DefaultValue", DefaultValue);
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
