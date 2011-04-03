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
using Common.NetworkPackage;

namespace Common
{
    /// <summary>
    /// Represents settings for a server.
    /// </summary>
    public class SettingsBase : DictionaryBase<string, object>
    {
        /// <summary>
        /// Gets the static instance.
        /// </summary>
        public static SettingsBase Instance { get; set; }

        /// <summary>
        /// Gets or sets the root storage location.
        /// </summary>
        /// <value>
        /// The root storage location.
        /// </value>
        public string RootStorageLocation
        {
            get
            {
                if (ContainsKey("RootStorageLocation"))
                    return (string)this["RootStorageLocation"];
                else
                    return @"C:\ClientDataStore\";
            }
            set
            {
                if (ContainsKey("RootStorageLocation"))
                    this["RootStorageLocation"] = value;
                else
                    Add("RootStorageLocation", value);
            }
        }

        /// <summary>
        /// Gets or sets the server ip.
        /// </summary>
        /// <value>
        /// The server ip.
        /// </value>
        public string PostgresConnectionString
        {
            get
            {
                if (ContainsKey("PostgresConnectionString"))
                    return (string)this["PostgresConnectionString"];
                else
                    return "Server=127.0.0.1;Port=5432;User Id=test;Password=test;Database=opendms;";
            }
            set
            {
                if (ContainsKey("PostgresConnectionString"))
                    this["PostgresConnectionString"] = value;
                else
                    Add("PostgresConnectionString", value);
            }
        }

        /// <summary>
        /// Gets or sets the server ip.
        /// </summary>
        /// <value>
        /// The server ip.
        /// </value>
        public string ServerIp
        {
            get
            {
                if (ContainsKey("ServerIp"))
                    return (string)this["ServerIp"];
                else
                    return "127.0.0.1";
            }
            set
            {
                if (ContainsKey("ServerIp"))
                    this["ServerIp"] = value;
                else
                    Add("ServerIp", value);
            }
        }

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        /// <value>
        /// The server port.
        /// </value>
        public int ServerPort
        {
            get
            {
                if (ContainsKey("ServerPort"))
                    return (int)this["ServerPort"];
                else
                    return 80;
            }
            set
            {
                if (ContainsKey("ServerPort"))
                    this["ServerPort"] = value;
                else
                    Add("ServerPort", value);
            }
        }

        /// <summary>
        /// Gets or sets the couchdb server ip.
        /// </summary>
        /// <value>
        /// The server ip.
        /// </value>
        public string CouchServerIp
        {
            get
            {
                if (ContainsKey("CouchServerIp"))
                    return (string)this["CouchServerIp"];
                else
                    return "127.0.0.1";
            }
            set
            {
                if (ContainsKey("CouchServerIp"))
                    this["CouchServerIp"] = value;
                else
                    Add("CouchServerIp", value);
            }
        }

        /// <summary>
        /// Gets or sets the couchdb server port.
        /// </summary>
        /// <value>
        /// The server port.
        /// </value>
        public int CouchServerPort
        {
            get
            {
                if (ContainsKey("CouchServerPort"))
                    return (int)this["CouchServerPort"];
                else
                    return 80;
            }
            set
            {
                if (ContainsKey("CouchServerPort"))
                    this["CouchServerPort"] = value;
                else
                    Add("CouchServerPort", value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the network buffer.
        /// </summary>
        /// <value>
        /// The size of the network buffer.
        /// </value>
        public int NetworkBufferSize
        {
            get
            {
                if (ContainsKey("NetworkBufferSize"))
                    return (int)this["NetworkBufferSize"];
                else
                    return 4096;
            }
            set
            {
                if (ContainsKey("NetworkBufferSize"))
                    this["NetworkBufferSize"] = value;
                else
                    Add("NetworkBufferSize", value);
            }
        }

        /// <summary>
        /// Gets or sets the network timeout duration in milliseconds.
        /// </summary>
        /// <value>
        /// The network timeout duration in milliseconds.
        /// </value>
        public int NetworkTimeout
        {
            get
            {
                if (ContainsKey("NetworkTimeout"))
                    return (int)this["NetworkTimeout"];
                else
                    return 5000;
            }
            set
            {
                if (ContainsKey("NetworkTimeout"))
                    this["NetworkTimeout"] = value;
                else
                    Add("NetworkTimeout", value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBase"/> class.
        /// </summary>
        public SettingsBase() 
            : base("Settings")
        {
        }

        /// <summary>
        /// Saves this instance to disk.
        /// </summary>
        /// <param name="relativeFilepath">The relative filepath.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public void Save(string relativeFilepath, FileSystem.IO fileSystem)
        {
            SaveToFile(relativeFilepath, fileSystem, true);
        }

        /// <summary>
        /// Loads a <see cref="SettingsBase"/> from the specified relative filepath.
        /// </summary>
        /// <param name="relativeFilepath">The relative filepath.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        /// <returns>A <see cref="SettingsBase"/>.</returns>
        public static SettingsBase Load(string relativeFilepath, FileSystem.IO fileSystem)
        {
            SettingsBase sb = new SettingsBase();

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
