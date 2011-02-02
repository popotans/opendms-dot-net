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
    /// An abstract class that represents the base requirements for any inheriting class
    /// </summary>
    public abstract class Base : IBase, System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>
        /// Serializes this instance using the specified XML writer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <returns>The XML writer passed in argument.</returns>
        public abstract XmlWriter Serialize(XmlWriter xmlWriter);
        /// <summary>
        /// Deserializes the content of the specified XML reader populating the properties of this instance.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <returns>The XML reader passed in argument.</returns>
        public abstract XmlReader Deserialize(XmlReader xmlReader);

        /// <summary>
        /// Initializes a new instance of the <see cref="Base"/> class.
        /// </summary>
        public Base()
        {
        }

        /// <summary>
        /// Saves this object to the local file system using version scheme naming.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="resource">A reference to the <see cref="FileSystem.MetaResource"/> for this object.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> used to document events.</param>
        /// <param name="overwrite">if set to <c>true</c> then any existing file should be overwritten; otherwise, <c>false</c>.</param>
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

        /// <summary>
        /// Saves this object to the file system.
        /// </summary>
        /// <param name="resource">A reference to the <see cref="FileSystem.MetaResource"/> for this object.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> used to document events.</param>
        /// <param name="overwrite">if set to <c>true</c> then any existing file should be overwritten; otherwise, <c>false</c>.</param>
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

        /// <summary>
        /// Saves this object to a specific file.
        /// </summary>
        /// <param name="relativePath">The relative path of the file to which this object is saved.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> providing the file system access.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> used to document events.</param>
        /// <param name="overwrite">if set to <c>true</c> then any existing file should be overwritten; otherwise, <c>false</c>.</param>
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

        /// <summary>
        /// Reads the specified resource into this object from disk.
        /// </summary>
        /// <param name="resource">A reference to the <see cref="FileSystem.MetaResource"/> for this object.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> used to document events.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
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

                throw new Exception(e.Message, e.InnerException);
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

        /// <summary>
        /// Reads the specified file into this object from disk.
        /// </summary>
        /// <param name="relativePath">The relative path of the file which will be read.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> providing the file system access.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> used to document events.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
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

            if (iostream == null)
                throw new FileNotFoundException();

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

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>A <see cref="MemoryStream"/> with XML data representing this object serialized.</returns>
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

        /// <summary>
        /// Deserializes the XML content contained within the argument <see cref="MemoryStream"/> to this object.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> containing the XML content.</param>
        public virtual void Deserialize(MemoryStream ms)
        {
            ms.Position = 0;
            XmlReader xmlReader = XmlReader.Create(ms, new XmlReaderSettings() { IgnoreWhitespace = true });

            Deserialize(xmlReader);

            xmlReader.Close();
        }

        /// <summary>
        /// Deserializes the XML content contained within the argument <see cref="Stream"/> to this object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the XML content.</param>
        public virtual void Deserialize(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
            XmlReader xmlReader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreWhitespace = true });

            Deserialize(xmlReader);

            xmlReader.Close();
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return 
        /// null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the 
        /// <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced 
        /// by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed 
        /// by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
        public void ReadXml(XmlReader reader)
        {
            Deserialize(reader);
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(XmlWriter writer)
        {
            Serialize(writer);
        }
    }
}
