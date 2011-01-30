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

namespace WindowsClient
{
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlIgnore]
        public bool SettingsFileExists 
        { 
            get 
            {
                return File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.xml");
            }
        }

        public string StorageLocation;

        public Settings()
        {
            StorageLocation = @"C:\ClientDataStore\";
        }

        /// <summary>
        /// Save settings to the file backing store
        /// </summary>
        public void Save()
        {
            if (StorageLocation.EndsWith("/"))
                StorageLocation = StorageLocation.TrimEnd('/') + Path.DirectorySeparatorChar;
            else if(!StorageLocation.EndsWith(Path.DirectorySeparatorChar.ToString()))
                StorageLocation += Path.DirectorySeparatorChar;
            XmlSerializer s = new XmlSerializer(typeof(Settings));
            TextWriter w = new StreamWriter(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.xml");
            s.Serialize(w, this);
            w.Close();
        }

        /// <summary>
        /// Load settings from the file backing store
        /// </summary>
        /// <returns></returns>
        public static Settings Load()
        {
            Settings settings = new Settings();
            string filepath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.xml";
            
            if (File.Exists(filepath))
            {
                XmlSerializer s = new XmlSerializer(typeof(Settings));
                TextReader r = new StreamReader(filepath);
                settings = (Settings)s.Deserialize(r);
                r.Close();

                if (settings.StorageLocation.EndsWith("/"))
                    settings.StorageLocation = settings.StorageLocation.TrimEnd('/') + Path.DirectorySeparatorChar;
                else if (!settings.StorageLocation.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    settings.StorageLocation += Path.DirectorySeparatorChar;
            }
            else
            {
                settings = new Settings();
                //settings.Save();
            }

            return settings;
        }
    }
}
