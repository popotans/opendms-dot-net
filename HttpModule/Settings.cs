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

namespace HttpModule
{
    /// <summary>
    /// Represents the settings for this HttpModule.
    /// </summary>
    public class Settings 
        : Common.SettingsBase
    {
        /// <summary>
        /// A global instance of this class.
        /// </summary>
        public new static Settings Instance
        {
            get
            {
                return (Settings)Common.SettingsBase.Instance;
            }
            set
            {
                Common.SettingsBase.Instance = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the file buffer.
        /// </summary>
        /// <value>
        /// The size of the file buffer.
        /// </value>
        public int FileBufferSize
        {
            get
            {
                if (ContainsKey("FileBufferSize"))
                    return (int)this["FileBufferSize"];
                else
                    return 20480;
            }
            set
            {
                if (ContainsKey("FileBufferSize"))
                    this["FileBufferSize"] = value;
                else
                    Add("FileBufferSize", value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings() 
            : base()
        {
            FileBufferSize = FileBufferSize;
            RootStorageLocation = @"C:\DataStore\";
            PostgresConnectionString = PostgresConnectionString;
            ServerIp = ServerIp;
            ServerPort = ServerPort;
            CouchServerIp = CouchServerIp;
            CouchServerPort = CouchServerPort;
            NetworkBufferSize = NetworkBufferSize;
            NetworkTimeout = NetworkTimeout;
        }

        /// <summary>
        /// Loads a <see cref="Settings"/> from the specified relative filepath.
        /// </summary>
        /// <param name="relativeFilepath">The relative filepath.</param>
        /// <param name="fileSystem">A reference to a <see cref="Common.FileSystem.IO"/>.</param>
        /// <returns>A <see cref="Settings"/>.</returns>
        public new static Settings Load(string relativeFilepath, Common.FileSystem.IO fileSystem)
        {
            Settings sb = new Settings();

            try
            {
                if (sb.ReadFromFile(relativeFilepath, fileSystem))
                    return sb;
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            return null;
        }
    }
}
