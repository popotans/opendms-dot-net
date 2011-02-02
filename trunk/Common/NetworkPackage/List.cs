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
    /// Represents a serializable List&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class List<T> 
        : ListBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="List&lt;T&gt;"/> class.
        /// </summary>
        public List()
            : base("List")
        {
        }
    }
}
