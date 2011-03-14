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

namespace Common.CouchDB
{
    /// <summary>
    /// Thrown when there is an exception encountered within the network code of the CouchDB project which is caused or caught by the project itself
    /// </summary>
    public class NetException : Exception
    {
        /// <summary>
        /// Thrown when there is an exception encountered within the network code of the CouchDB project which is caused or caught by the project itself
        /// </summary>
        public NetException()
            : base()
        {
        }

        /// <summary>
        /// Thrown when there is an exception encountered within the network code of the CouchDB project which is caused or caught by the project itself
        /// </summary>
        /// <param name="message">The exception message</param>
        public NetException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Thrown when there is an exception encountered within the network code of the CouchDB project which is caused or caught by the project itself
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public NetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
