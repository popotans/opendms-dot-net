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

namespace Common.NetworkPackage
{
    public class Server 
        : DictionaryBase<string, object>
    {
        public Server()
            : base("Server")
        {
        }

        public bool Validate()
        {
            if (!ContainsKey("Name")) return false;
            if (!ContainsKey("Location")) return false;
            if (!ContainsKey("Address")) return false;
            if (!ContainsKey("Port")) return false;

            if (this["Name"].GetType() != typeof(string)) return false;
            if (this["Location"].GetType() != typeof(string)) return false;
            if (this["Address"].GetType() != typeof(string)) return false;
            if (this["Port"].GetType() != typeof(int)) return false;

            return true;
        }
    }
}
