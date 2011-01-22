using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Common.NetworkPackage
{
    [XmlRoot("FieldCollection")]
    public class MetaFieldCollection : SerializableDictionary
    {
    }
}
