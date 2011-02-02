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

namespace Common.FileSystem
{
    /// <summary>
    /// An enumeration of types of a Resource.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// An unknown resource type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// <see cref="MetaResource"/>
        /// </summary>
        Meta = 1,
        /// <summary>
        /// <see cref="DataResource"/>
        /// </summary>
        Data = 2
    }
}
