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

namespace HttpModule.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// The process failed.
        /// </summary>
        Failure = 0,
        /// <summary>
        /// The process was successful.
        /// </summary>
        Success,
        /// <summary>
        /// The resource was not found.
        /// </summary>
        NotFound,
        /// <summary>
        /// An I/O error occurred while processing.
        /// </summary>
        IOError,
        /// <summary>
        /// An error occurred while serializing.
        /// </summary>
        SerializationError,
        /// <summary>
        /// An error occurred while instantiating an object.
        /// </summary>
        InstantiationError,
        /// <summary>
        /// The resource is locked.
        /// </summary>
        ResourceIsLocked,
        /// <summary>
        /// The user does not have permission to access the resource.
        /// </summary>
        PermissionsError,
        /// <summary>
        /// The length of the data does not match the length specified in the <see cref="Common.Data.MetaAsset"/>.
        /// </summary>
        LengthMismatch,
        /// <summary>
        /// The MD5 of the data does not match the MD5 in the <see cref="Common.Data.MetaAsset"/>.
        /// </summary>
        Md5Mismatch
    }
}
