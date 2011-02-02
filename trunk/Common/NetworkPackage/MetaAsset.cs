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
using System.Xml;
using System.Text;

namespace Common.NetworkPackage
{
    /// <summary>
    /// Provides a method of packaging and unpackaging a <see cref="Data.MetaAsset"/> object
    /// using XML for storage and transfer.
    /// </summary>
    public class MetaAsset : DictionaryBase<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaAsset"/> class.
        /// </summary>
        public MetaAsset() 
            : base("MetaAsset")
        {
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            if (!ContainsKey("$guid")) return false;
            if (!ContainsKey("$etag")) return false;
            if (!ContainsKey("$metaversion")) return false;
            if (!ContainsKey("$dataversion")) return false;
            if (!ContainsKey("$creator")) return false;
            if (!ContainsKey("$length")) return false;
            if (!ContainsKey("$md5")) return false;
            if (!ContainsKey("$extension")) return false;
            if (!ContainsKey("$created")) return false;
            if (!ContainsKey("$modified")) return false;
            if (!ContainsKey("$lastaccess")) return false;
            if (!ContainsKey("$title")) return false;
            if (!ContainsKey("$tags")) return false;

            if (this["$guid"].GetType() != typeof(Guid)) return false;
            if (this["$etag"].GetType() != typeof(string)) return false;
            if (this["$metaversion"].GetType() != typeof(uint)) return false;
            if (this["$dataversion"].GetType() != typeof(uint)) return false;
            if (ContainsKey("$lockedby")) { if (this["$lockedby"].GetType() != typeof(string)) return false; }
            if (ContainsKey("$lockedat")) { if (this["$lockedat"].GetType() != typeof(DateTime?)) return false; }
            if (this["$creator"].GetType() != typeof(string)) return false;
            if (this["$length"].GetType() != typeof(ulong)) return false;
            if (this["$md5"].GetType() != typeof(string)) return false;
            if (this["$extension"].GetType() != typeof(string)) return false;
            if (this["$created"].GetType() != typeof(DateTime)) return false;
            if (this["$modified"].GetType() != typeof(DateTime)) return false;
            if (this["$lastaccess"].GetType() != typeof(DateTime)) return false;
            if (this["$title"].GetType() != typeof(string)) return false;
            if (this["$tags"].GetType() != typeof(System.Collections.Generic.List<string>)) return false;

            return true;
        }
    }
}
