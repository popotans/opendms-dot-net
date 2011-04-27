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

namespace WindowsClient
{
    /// <summary>
    /// Represents a data item (row) to be displayed on a data grid.
    /// </summary>
    public class SearchWindowDataItem
    {
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score (0-1).
        /// </value>
        public string Score { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="Guid"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Guid"/>.
        /// </value>
        public string Guid { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public string Extension { get; set; }
        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        /// <value>
        /// The person responsible for version creation.
        /// </value>
        public string Creator { get; set; }
    }
}
