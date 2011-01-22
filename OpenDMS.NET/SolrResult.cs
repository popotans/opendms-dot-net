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
