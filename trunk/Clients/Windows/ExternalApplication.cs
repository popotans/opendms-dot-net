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
using Microsoft.Win32;

namespace WindowsClient
{
    /// <summary>
    /// Provides the ability to interact with external applications.
    /// </summary>
    public class ExternalApplication
    {
        /// <summary>
        /// Opens the file with default application.
        /// </summary>
        /// <param name="dataAsset">The data asset.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public static bool OpenFileWithDefaultApplication(Common.Data.DataAsset dataAsset, out string errorMessage)
        {
            RegistryKey root, key;
            string progid, syntax;

            // Does the extension exist?
            try
            {
                root = Registry.ClassesRoot;
                if ((key = root.OpenSubKey(dataAsset.Resource.Extension)) == null)
                {
                    errorMessage = "There is no application associated with the resource's extension.";
                    return false;
                }
                progid = (string)key.GetValue(null, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (string.IsNullOrEmpty(progid))
                {
                    errorMessage = "There is no application associated with the resource's extension.";
                    return false;
                }
            }
            catch(Exception e)
            {
                errorMessage = e.Message;
                return false;
            }

            try
            {
                // Does the ProgID exist?
                if ((key = root.OpenSubKey(progid)) == null)
                {
                    errorMessage = "The program associated with the extension does not have a registry entry.";
                    return false;
                }
                // Does the shell subkey exist?
                if ((key = key.OpenSubKey("shell")) == null)
                {
                    errorMessage = "The program associated with the extension does not indicate its supported commands.";
                    return false;
                }
                // Does the open subkey exist
                if ((key = key.OpenSubKey("open")) == null)
                {
                    errorMessage = "The program associated with the extension does not indicate support for the open command.";
                    return false;
                }
                // Does the command subkey exist
                if ((key = key.OpenSubKey("command")) == null)
                {
                    errorMessage = "The program associated with the extension does not indicate the syntax of the open command.";
                    return false;
                }
                // Get the value
                syntax = (string)key.GetValue(null, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                if (string.IsNullOrEmpty(syntax))
                {
                    errorMessage = "The program associated with the extension does not indicate the syntax of the open command.";
                    return false;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }

            // Now, parse the syntax
            // ex: "C:\Program Files (x86)\OpenOffice.org 3\program\swriter.exe" -o "%1"
            // ex: "C:\Program Files (x86)\Common Files\Microsoft Shared\MSEnv\VSLauncher.exe" "%1"


            System.Diagnostics.Process p = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\OpenOffice.org 3\program\swriter.exe", " -o " +
                MainWindow.FileSystem.GetFullFilePath(dataAsset.Resource.RelativeFilepath));

            //System.Diagnostics.Process p = new System.Diagnostics.Process();
            //p.StartInfo.RedirectStandardOutput = false;
            ////p.StartInfo.FileName = filepath;
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.Verb = "open";
            //p.Start();
            p.WaitForExit();
            p.Dispose();

            errorMessage = null;
            return true;
        }

    }
}
