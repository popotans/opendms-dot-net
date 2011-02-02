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
    /// <summary>
    /// Provides a method of packaging and unpackaging a <see cref="Data.ETag"/> object 
    /// using XML for storage and transfer.
    /// </summary>
    public class ETag 
        : DictionaryBase<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ETag"/> class.
        /// </summary>
        public ETag()
            : base("ETag")
        {
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            if (!ContainsKey("Value")) return false;

            if (this["Value"].GetType() != typeof(string)) return false;

            return true;
        }
    }
}
