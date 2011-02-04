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

namespace WindowsClient
{
    /// <summary>
    /// Provides simple and commonly useful utility methods
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Makes the file size human readable.
        /// </summary>
        /// <param name="bytes">The quantity of bytes.</param>
        /// <returns>A string representing the file size.</returns>
        public static string MakeBytesHumanReadable(long bytes)
        {
            string size;

            if (bytes > 1073741824)
                size = string.Format("{0:#.##} GB", ((double)bytes / 1073741824D));
            else if (bytes > 1048576)
                size = string.Format("{0:#.##} MB", ((double)bytes / 1048576D));
            else if (bytes > 1024)
                size = string.Format("{0:#.##} KB", ((double)bytes / 1024D));
            else
                size = string.Format("{0:#} Bytes", bytes);

            return size;
        }

        /// <summary>
        /// Makes the file size human readable.
        /// </summary>
        /// <param name="bytes">The quantity of bytes.</param>
        /// <returns>A string representing the file size.</returns>
        public static string MakeBytesHumanReadable(ulong bytes)
        {
            string size;

            if (bytes > 1073741824)
                size = string.Format("{0:#.##} GB", ((double)bytes / 1073741824D));
            else if (bytes > 1048576)
                size = string.Format("{0:#.##} MB", ((double)bytes / 1048576D));
            else if (bytes > 1024)
                size = string.Format("{0:#.##} KB", ((double)bytes / 1024D));
            else
                size = string.Format("{0:#} Bytes", bytes);

            return size;
        }

        /// <summary>
        /// Gets the application's path.
        /// </summary>
        /// <returns></returns>
        public static string GetAppPath()
        {
            string str = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (!str.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) 
                str += System.IO.Path.DirectorySeparatorChar.ToString();
            return str;
        }
    }
}
