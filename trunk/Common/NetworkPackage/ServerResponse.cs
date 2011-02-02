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
using System.Text;

namespace Common.NetworkPackage
{
    /// <summary>
    /// Represents a server's response to a client's request.
    /// </summary>
    public class ServerResponse : DictionaryBase<string, object>
    {
        /// <summary>
        /// An enumeration of error codes.
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>
            /// No error code set.
            /// </summary>
            None = 0,
            /// <summary>
            /// A lease already exists.
            /// </summary>
            ExistingLease,
            /// <summary>
            /// A resource already exists.
            /// </summary>
            ExistingResource,
            /// <summary>
            /// The resource is locked.
            /// </summary>
            ReasourceIsLocked,
            /// <summary>
            /// The Guid is invalid.
            /// </summary>
            InvalidGuid,
            /// <summary>
            /// The read only value is invalid.
            /// </summary>
            InvalidReadOnlyValue,
            /// <summary>
            /// The formatting is invalid.
            /// </summary>
            InvalidFormatting,
            /// <summary>
            /// The permissions are invalid.
            /// </summary>
            InvalidPermissions,
            /// <summary>
            /// The search parameters are invalid.
            /// </summary>
            InvalidSearchParameters,
            /// <summary>
            /// The indexing failed.
            /// </summary>
            FailedIndexing,
            /// <summary>
            /// The resource does not exist.
            /// </summary>
            ResourceDoesNotExist,
            /// <summary>
            /// An unhandled exception occurred.
            /// </summary>
            Exception
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResponse"/> class.
        /// </summary>
        public ServerResponse()
            : base("ServerResponse")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResponse"/> class.
        /// </summary>
        /// <param name="pass">If set to <c>true</c> the response is considered successful; otherwise, <c>false</c>.</param>
        /// <param name="code">The <see cref="ErrorCode"/>.</param>
        public ServerResponse(bool pass, ErrorCode code)
            : base("ServerResponse")
        {
            Add("Pass", pass);
            Add("ErrorCode", code);
            Add("Timestamp", DateTime.Now);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerResponse"/> class.
        /// </summary>
        /// <param name="pass">If set to <c>true</c> the response is considered successful; otherwise, <c>false</c>.</param>
        /// <param name="code">The <see cref="ErrorCode"/>.</param>
        /// <param name="message">A message describing the error.</param>
        public ServerResponse(bool pass, ErrorCode code, string message)
            : base("ServerResponse")
        {
            Add("Pass", pass);
            Add("ErrorCode", code);
            Add("Message", message);
            Add("Timestamp", DateTime.Now);
        }
    }
}
