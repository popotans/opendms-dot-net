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

namespace OpenDMS
{
    public class Utilities
    {
        public static long Timestamp(DateTime dt)
        {
            return Convert.ToInt64(dt.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        public static DateTime DateTimeFromTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(Convert.ToDouble(timestamp));
        }

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
    }
}
