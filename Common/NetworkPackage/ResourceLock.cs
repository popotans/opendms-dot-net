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

namespace Common.NetworkPackage
{
    public class ResourceLock : DictionaryBase<string, object>
    {
        public ResourceLock()
            : base("ResourceLock")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("Guid")) return false;
            if (!ContainsKey("Timestamp")) return false;
            if (!ContainsKey("Username")) return false;

            if (this["Guid"].GetType() != typeof(Guid)) return false;
            if (this["Timestamp"].GetType() != typeof(DateTime)) return false;
            if (this["Username"].GetType() != typeof(string)) return false;

            return true;
        }
    }
}
