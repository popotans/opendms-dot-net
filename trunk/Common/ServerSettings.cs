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
using System.Xml.Serialization;

namespace Common
{
    /// <summary>
    /// Represents settings for a server.
    /// </summary>
    [XmlRoot("ServerSettings")]
    public class ServerSettings
    {
        /// <summary>
        /// A reference to the static instance.
        /// </summary>
        private static ServerSettings _instance;
        /// <summary>
        /// The IP address of the server.
        /// </summary>
        private string _serverIp;
        /// <summary>
        /// The port number of the service.
        /// </summary>
        private int _serverPort;
        /// <summary>
        /// The size of the network buffer.
        /// </summary>
        private int _networkBufferSize;
        /// <summary>
        /// The timeout duration.
        /// </summary>
        private int _networkTimeout;

        /// <summary>
        /// Gets the static instance.
        /// </summary>
        [XmlIgnore]
        public static ServerSettings Instance
        {
            get
            {
                string filepath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ServerSettings.xml";

                if (_instance == null)
                {
                    if (File.Exists(filepath))
                        _instance = Load();
                    else
                    {
                        _instance = new ServerSettings();
                        _instance.Save();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is loaded; otherwise, <c>false</c>.
        /// </value>
        [XmlIgnore]
        public bool IsLoaded { get; set; }

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
                return _serverIp;
            }
            set
            {
                _serverIp = value;
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
                return _serverPort;
            }
            set
            {
                _serverPort = value;
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
                return _networkBufferSize;
            }
            set
            {
                _networkBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the network timeout duration.
        /// </summary>
        /// <value>
        /// The network timeout duration.
        /// </value>
        public int NetworkTimeout
        {
            get
            {
                return _networkTimeout;
            }
            set
            {
                _networkTimeout = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSettings"/> class.
        /// </summary>
        public ServerSettings()
        {
            _serverIp = "127.0.0.1";
            _serverPort = 80;
            _networkBufferSize = 4096;
            _networkTimeout = 5000;
        }

        /// <summary>
        /// Save settings to the file backing store
        /// </summary>
        public void Save()
        {
            XmlSerializer s = new XmlSerializer(typeof(ServerSettings));
            TextWriter w = new StreamWriter(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ServerSettings.xml");
            s.Serialize(w, this);
            w.Close();
        }

        /// <summary>
        /// Load settings from the file backing store
        /// </summary>
        /// <returns></returns>
        public static ServerSettings Load()
        {
            ServerSettings settings = new ServerSettings();
            string filepath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ServerSettings.xml";

            if (filepath == null)
                throw new ArgumentNullException("filepath", "The argument filepath cannot be null.");

            if (File.Exists(filepath))
            {
                settings.IsLoaded = true;
                XmlSerializer s = new XmlSerializer(typeof(ServerSettings));
                TextReader r = new StreamReader(filepath);
                settings = (ServerSettings)s.Deserialize(r);
                r.Close();
            }
            else
            {
                settings = new ServerSettings();
                //settings.Save();
            }

            return settings;
        }
    }
}
