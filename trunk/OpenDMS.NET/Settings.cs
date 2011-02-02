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
using System.Net;

namespace HttpModule
{
    /// <summary>
    /// Represents the settings for this HttpModule.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// A global instance of this class.
        /// </summary>
        public static Settings Instance = new Settings();

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        public IPEndPoint Host { get; set; }
        /// <summary>
        /// Gets or sets the storage location.
        /// </summary>
        /// <value>
        /// The storage location.
        /// </value>
        public string StorageLocation { get; set; }
        /// <summary>
        /// Gets or sets the lease expiration.
        /// </summary>
        /// <value>
        /// The lease expiration.
        /// </value>
        public long LeaseExpiration { get; set; }
        /// <summary>
        /// Gets or sets the size of the file buffer.
        /// </summary>
        /// <value>
        /// The size of the file buffer.
        /// </value>
        public int FileBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the search host.
        /// </summary>
        /// <value>
        /// The search host.
        /// </value>
        public IPEndPoint SearchHost { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            Host = new IPEndPoint(IPAddress.Parse("192.168.1.103"), 9160);
            StorageLocation = @"C:\DataStore\";
            LeaseExpiration = 900000; // 15 minutes
            FileBufferSize = 20480;
            SearchHost = new IPEndPoint(IPAddress.Parse("192.168.1.50"), 8080);
        }

    }
}
