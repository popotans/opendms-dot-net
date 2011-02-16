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

namespace Common.NetworkPackage
{
    /// <summary>
    /// Provides serialization and I/O routes for implementing classes.
    /// </summary>
    public interface IBase
    {
        /// <summary>
        /// Serializes this instance and returns a <see cref="MemoryStream"/> containing the XML 
        /// formatted content.
        /// </summary>
        /// <returns>A <see cref="MemoryStream"/> containing the XML 
        /// formatted content.</returns>
        MemoryStream Serialize();
        /// <summary>
        /// Serializes this instance returning the XML writer argument.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <returns>The XML writer argument.</returns>
        XmlWriter Serialize(XmlWriter xmlWriter);
        /// <summary>
        /// Deserializes the content XML from the specified <see cref="MemoryStream"/> and populating 
        /// this instance based on the content.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> to deserialize the content from.</param>
        void Deserialize(MemoryStream ms);
        /// <summary>
        /// Deserializes the content XML from the specified <see cref="Stream"/> and populating 
        /// this instance based on the content.
        /// </summary>
        /// <param name="stream">The stream.</param>
        void Deserialize(Stream stream);
        /// <summary>
        /// Deserializes the content XML from the specified XML reader and populating this instance
        /// based on the content.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <returns>The XML reader argument.</returns>
        XmlReader Deserialize(XmlReader xmlReader);
        /// <summary>
        /// Saves this object to the file system.
        /// </summary>
        /// <param name="resource">The <see cref="FileSystem.MetaResource"/> providing file system access.</param>
        /// <param name="overwrite">if set to <c>true</c> overwrite any existing file; otherwise, <c>false</c>.</param>
        void Save(FileSystem.MetaResource resource, bool overwrite);
        /// <summary>
        /// Loads this object from the file system.
        /// </summary>
        /// <param name="resource">The <see cref="FileSystem.MetaResource"/> providing file system access.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        bool Read(FileSystem.MetaResource resource);
    }
}
