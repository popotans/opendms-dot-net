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

namespace Common.Data
{
    /// <summary>
    /// Provides a data representation of the header information returned from the server as the result 
    /// of a HTTP HEAD request.
    /// </summary>
    public class Head
    {
        /// <summary>
        /// Gets or sets the <see cref="ETag"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ETag"/>.
        /// </value>
        public ETag ETag { get; set; }
        /// <summary>
        /// Gets or sets a string representation of the MD5.
        /// </summary>
        /// <value>
        /// The MD5 value.
        /// </value>
        public string MD5 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/>.</param>
        /// <param name="md5">The MD5.</param>
        public Head(ETag etag, string md5)
        {
            ETag = etag;
            MD5 = md5;
        }
    }
}
