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
using System.Collections.Generic;

namespace HttpModule
{
    /// <summary>
    /// 
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Gets a numeric representation of the amount of milliseconds since 1/1/1970 to the argument <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/>.</param>
        /// <returns>The amount of milliseconds since 1/1/1970.</returns>
        public static long Timestamp(DateTime dt)
        {
            return Convert.ToInt64(dt.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        /// <summary>
        /// Converts a timestamp to a <see cref="DateTime"/> representation.
        /// </summary>
        /// <param name="timestamp">The milliseconds since 1/1/1970.</param>
        /// <returns>A <see cref="DateTime"/> representation.</returns>
        public static DateTime DateTimeFromTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(Convert.ToDouble(timestamp));
        }

        /// <summary>
        /// Converts a <see cref="System.IO.Stream"/> to a UTF-8 formatted string.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/>.</param>
        /// <returns>A UTF-8 formatted string.</returns>
        public static string StreamToUtf8String(System.IO.Stream stream)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[40960];
            string str = "";

            if(!stream.CanRead)
                throw new System.IO.IOException("Cannot read from stream.");

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                str += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return str;
        }

        /// <summary>
        /// Gets the application's path.
        /// </summary>
        /// <returns></returns>
        public static string GetAppPath()
        {
            string str = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

            if (!str.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                str += System.IO.Path.DirectorySeparatorChar.ToString();
            return str;
        }
    }
}
