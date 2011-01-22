using System;
using System.IO;
using System.Xml.Serialization;

namespace WindowsClient
{
    [XmlRoot("Settings")]
    public class Settings
    {
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
                settings.Save();
            }

            return settings;
        }
    }
}
