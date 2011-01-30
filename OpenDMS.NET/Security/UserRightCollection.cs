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

namespace OpenDMS.Security
{
    public class UserRightCollection 
        : Dictionary<string, int>
    {
        public byte[] ToBytes()
        {
            string total = "";
            Dictionary<string, int>.Enumerator en = GetEnumerator();

            while (en.MoveNext())
            {
                total += en.Current.Key + ":" + en.Current.Value.ToString() + ",";
            }

            if (total.Length > 0)
                total = total.Trim(new char[] { ',' });

            return System.Text.Encoding.UTF8.GetBytes(total);
        }

        public void FromString(string str)
        {
            string[] vals;
            string[] entries = str.Split(',');

            foreach (string entry in entries)
            {
                vals = entry.Split(':');
                Add(vals[0], int.Parse(vals[1]));
            }
        }
    }
}
