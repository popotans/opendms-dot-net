using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    public abstract class Base : IBase, System.Xml.Serialization.IXmlSerializable
    {
        public abstract XmlWriter Serialize(XmlWriter xmlWriter);
        public abstract XmlReader Deserialize(XmlReader xmlReader);

        public Base()
        {
        }

        public void SaveUsingVersionScheme(UInt64 version, FileSystem.MetaResource resource, 
            Logger logger, bool overwrite)
        {
            MemoryStream ms;
            FileSystem.IOStream iostream;

            // Make directory
            resource.CreateContainingDirectory();

            // Verify overwrite
            if (resource.ExistsOnFilesystem() && !overwrite)
                return;

            try
            {
                iostream = resource.GetExclusiveWriteStreamUsingVersionScheme(version,
                    "Common.NetworkPackage.Base.SaveUsingVersionScheme()");
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                ms = Serialize();
            }
            catch (Exception e)
            {
                resource.CloseStream();

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to serialize the object.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                iostream.CopyFrom(ms);
            }
            catch (Exception e)
            {
                resource.CloseStream();

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to transfer data from memory to the file system.\r\n" +
                        Logger.ExceptionToString(e));

                throw e;
            }

            resource.CloseStream();
        }

        public void Save(FileSystem.MetaResource resource, Logger logger, bool overwrite)
        {
            MemoryStream ms;
            FileSystem.IOStream iostream;

            // Make directory
            resource.CreateContainingDirectory();

            // Verify overwrite
            if (resource.ExistsOnFilesystem() && !overwrite)
                return;

            try
            {
                iostream = resource.GetExclusiveWriteStream("Common.NetworkPackage.Base.Save()");
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                ms = Serialize();
            }
            catch (Exception e)
            {
                resource.CloseStream();

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to serialize the object.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                iostream.CopyFrom(ms);
            }
            catch (Exception e)
            {
                resource.CloseStream();

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to transfer data from memory to the file system.\r\n" +
                        Logger.ExceptionToString(e));

                throw e;
            }

            resource.CloseStream();
        }

        public void SaveToFile(string relativePath, FileSystem.IO fileSystem, Logger logger, bool overwrite)
        {
            MemoryStream ms;
            FileSystem.IOStream iostream;

            // Make directory
            fileSystem.CreateDirectoryPath(System.IO.Path.GetDirectoryName(relativePath));

            // Verify overwrite
            if (fileSystem.ResourceExists(relativePath) && !overwrite)
                return;

            try
            {
                iostream = fileSystem.Open(relativePath, FileMode.Create, FileAccess.Write, 
                    FileShare.None, FileOptions.None, "Common.NetworkPackage.Base.SaveToFile()");
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to open a resource\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                ms = Serialize();
            }
            catch (Exception e)
            {
                fileSystem.Close(iostream);

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to serialize the object.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                iostream.CopyFrom(ms);
            }
            catch (Exception e)
            {
                fileSystem.Close(iostream);

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to transfer data from memory to the file system.\r\n" +
                        Logger.ExceptionToString(e));

                throw e;
            }

            fileSystem.Close(iostream);
        }

        public bool Read(FileSystem.MetaResource resource, Logger logger)
        {
            FileSystem.IOStream iostream;

            try
            {
                iostream = resource.GetExclusiveReadStream("Common.NetworkPackage.Base.Read()");
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to read from a resource.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                Deserialize(iostream.Stream);
            }
            catch (Exception e)
            {
                resource.CloseStream();

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to deserialize the resource.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            resource.CloseStream();

            return true;
        }

        public bool ReadFromFile(string relativePath, FileSystem.IO fileSystem, Logger logger)
        {
            FileSystem.IOStream iostream;

            try
            {
                iostream = fileSystem.Open(relativePath, FileMode.Open, FileAccess.Read, 
                    FileShare.None, FileOptions.None, "Common.NetworkPackage.Base.ReadFromFile");
            }
            catch (Exception e)
            {
                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to read from a resource.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            try
            {
                Deserialize(iostream.Stream);
            }
            catch (Exception e)
            {
                fileSystem.Close(iostream);

                if (logger != null)
                    logger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
                        "attempting to deserialize the resource.\r\n" + Logger.ExceptionToString(e));

                throw e;
            }

            fileSystem.Close(iostream);

            return true;
        }
        
        public virtual MemoryStream Serialize()
        {
            MemoryStream ms = new MemoryStream();
            XmlWriterSettings sets = new XmlWriterSettings();
            sets.Encoding = Encoding.UTF8;
            XmlWriter xmlWriter = XmlWriter.Create(ms, sets);
            xmlWriter.WriteStartDocument();

            Serialize(xmlWriter);

            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
            ms.Position = 0;
            return ms;
        }

        public virtual void Deserialize(MemoryStream ms)
        {
            ms.Position = 0;
            XmlReader xmlReader = XmlReader.Create(ms, new XmlReaderSettings() { IgnoreWhitespace = true });

            Deserialize(xmlReader);

            xmlReader.Close();
        }

        public virtual void Deserialize(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
            XmlReader xmlReader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreWhitespace = true });

            Deserialize(xmlReader);

            xmlReader.Close();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Deserialize(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            Serialize(writer);
        }
    }
}
