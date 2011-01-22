using System;
using System.IO;
using System.Xml;

namespace Common.NetworkPackage
{
    public interface IBase
    {
        MemoryStream Serialize();
        XmlWriter Serialize(XmlWriter xmlWriter);
        void Deserialize(MemoryStream ms);
        void Deserialize(Stream stream);
        XmlReader Deserialize(XmlReader xmlReader);
        void Save(FileSystem.MetaResource resource, Logger logger, bool overwrite);
        bool Read(FileSystem.MetaResource resource, Logger logger);
    }
}
