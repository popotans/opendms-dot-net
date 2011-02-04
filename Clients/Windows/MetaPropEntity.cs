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
    /// Represents an entry containing all requisite information for a <see cref="System.Windows.Controls.TreeViewItem"/> in the <see cref="MetaPropWindow"/>.
    /// </summary>
    public class MetaPropEntity
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is updated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is updated; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaPropEntity"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="title">The title.</param>
        /// <param name="value">The value.</param>
        /// <param name="isReadOnly">if set to <c>true</c> [is read only].</param>
        public MetaPropEntity(string key, string title, object value, bool isReadOnly)
        {
            Key = key;
            Title = title;
            Value = value;
            IsReadOnly = isReadOnly;
            IsUpdated = false;
        }
    }
}
