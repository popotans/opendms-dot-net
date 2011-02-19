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

namespace WindowsClient
{
    /// <summary>
    /// Represents settings for a client.
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
        /// Gets or sets the storage location.
        /// </summary>
        /// <value>
        /// The storage location.
        /// </value>
        public string StorageLocation
        {
            get
            {
                if (ContainsKey("StorageLocation"))
                    return (string)this["StorageLocation"];
                else
                    return @"C:\DataStore\";
            }
            set
            {
                if (ContainsKey("StorageLocation"))
                    this["StorageLocation"] = value;
                else
                    Add("StorageLocation", value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
            : base()
        {
        }

        /// <summary>
        /// Saves this instance to disk.
        /// </summary>
        /// <param name="fullFilepath">The full filepath where to save the file.</param>
        public void Save(string fullFilepath)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[20480];
            MemoryStream ms = new MemoryStream();
            FileStream fs;

            try
            {
                ms = Serialize();
            }
            catch (Exception e)
            {
                if(Common.Logger.General != null)
                    Common.Logger.General.Error("An exception occurred while attempting to serialize the settings.", e);
                return;
            }

            try
            {
                fs = new FileStream(fullFilepath, FileMode.Create, FileAccess.Write, FileShare.Read, 20480);
            }
            catch (Exception e)
            {
                if (Common.Logger.General != null)
                    Common.Logger.General.Error("An exception occurred while accessing the settings file on disk.", e);
                return;
            }

            try
            {
                while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                    fs.Write(buffer, 0, bytesRead);
            }
            catch (Exception e)
            {
                fs.Close();
                fs.Dispose();

                if (Common.Logger.General != null)
                    Common.Logger.General.Error("An exception occurred while writting to the settings file.", e);

                return;
            }

            if (Common.Logger.General != null)
                Common.Logger.General.Info("Settings saved.");

            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Loads a <see cref="Settings"/> from the specified relative filepath.
        /// </summary>
        /// <param name="fullFilepath">The full filepath where to save the file.</param>
        /// <returns>A <see cref="Settings"/>.</returns>
        public static Settings Load(string fullFilepath)
        {
            Settings sb = new Settings();
            FileStream s = null;

            if (!Directory.Exists(Path.GetDirectoryName(fullFilepath)))
                Directory.CreateDirectory(Path.GetDirectoryName(fullFilepath));

            try
            {
                s = new FileStream(fullFilepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            try
            {
                sb.Deserialize(s);
            }
            catch (Exception e)
            {
                s.Close();
                s.Dispose();
                Common.Logger.General.Error("An exception occurred while attempting to deserialize the resource.", e);
                return null;
            }

            s.Close();
            s.Dispose();

            return sb;
        }
    }
}
