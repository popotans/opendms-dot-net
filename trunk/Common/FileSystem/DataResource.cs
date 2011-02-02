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

namespace Common.FileSystem
{
    /// <summary>
    /// Represents a <see cref="Data.DataAsset"/> on the local file system.
    /// </summary>
    public class DataResource
        : ResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataResource"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> providing a unique reference to the Asset.</param>
        /// <param name="extension">The extension of the resource (e.g., .doc, .xsl, .odt)</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/> instance.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public DataResource(Guid guid, string extension, IO fileSystem, Logger logger)
            : base(guid, ResourceType.Data, extension, fileSystem, logger)
        {
        }
    }
}
