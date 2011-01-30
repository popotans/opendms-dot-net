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
using System.Xml;
using System.Collections.Generic;

namespace OpenDMS
{
    public class SolrResult
    {
        public Guid Id { get; set; }

        public static SolrResult Deserialize(XmlReader reader)
        {
            bool loop = true;
            SolrResult result = null;

            if (reader.IsEmptyElement)
                throw new ArgumentException("Invalid XML formatting.");

            if (reader.NodeType == XmlNodeType.None)
                reader.MoveToContent();

            while (loop)
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (string.IsNullOrEmpty(reader.LocalName))
                            reader.MoveToContent();

                        switch (reader.LocalName)
                        {
                            case "str":
                                if (reader.GetAttribute("name") == "id")
                                {
                                    string sguid = reader.ReadElementContentAsString();
                                    sguid = sguid.Substring(0, sguid.IndexOf('.'));
                                    result = new SolrResult() { Id = new Guid(sguid) };
                                }
                                else
                                    reader.Skip();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                        break;
                    case System.Xml.XmlNodeType.None:
                        loop = false;
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "doc")
                            return result;
                        else
                            throw new FormatException("Invalid XML formatting");
                    default:
                        reader.MoveToContent();
                        reader.Skip();
                        break;
                }
            }

            return result;
        }
    }
}
