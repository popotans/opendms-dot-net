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
    [XmlRoot("ServerSettings")]
    public class ServerSettings
    {
        private static ServerSettings _instance;
        private string _serverIp;
        private int _serverPort;
        private int _networkBufferSize;
        private int _networkTimeout;

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

        [XmlIgnore]
        public bool IsLoaded { get; set; }

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
