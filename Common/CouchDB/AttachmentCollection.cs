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
using System.Collections;
using System.Collections.Generic;

namespace Common.CouchDB
{
    /// <summary>
    /// Represents a collection of type <see cref="Attachment"/>.
    /// </summary>
    public class AttachmentCollection
    {
        private ArrayList _attachments;

        /// <summary>
        /// Constructor
        /// </summary>
        public AttachmentCollection()
        {
            _attachments = new ArrayList();
        }

        /// <summary>
        /// Gets the quantity of Attachments in the collection
        /// </summary>
        public int Count
        {
            get { return _attachments.Count; }
        }

        /// <summary>
        /// Gets/Sets the Attachment at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>The CouchDB.Attachment corresponding to the index</returns>
        public Attachment this[int index]
        {
            get { return (Attachment)_attachments[index]; }
            set { _attachments[index] = value; }
        }

        /// <summary>
        /// Adds a attachment to the collection
        /// </summary>
        /// <param name="att">The Attachment to add to the collection</param>
        public void Add(Attachment att)
        {
            _attachments.Add(att);
        }

        /// <summary>
        /// Removes the Attachment at the specified index
        /// </summary>
        /// <param name="index">A zero-based index of the element to remove</param>
        public void RemoveAt(int index)
        {
            lock (_attachments) { _attachments.RemoveAt(index); }
        }

        /// <summary>
        /// Convert to an array of Attachments
        /// </summary>
        /// <returns>An array of Attachments</returns>
        public Attachment[] ToArray()
        {
            return (Attachment[])_attachments.ToArray(typeof(Attachment));
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, object> Serialize()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            KeyValuePair<string, object> kvp;

            try
            {
                for (int i = 0; i < _attachments.Count; i++)
                {
                    kvp = this[i].Serialize();
                    dictionary.Add(kvp.Key, kvp.Value);
                }
            }
            catch (CouchDBException e)
            {
                throw new CouchDBException("An exception occurred while attempting to serialize an attachment, please refer to the inner exception for details.", e);
            }

            return new KeyValuePair<string, object>("_attachments", dictionary);
        }
    }
}