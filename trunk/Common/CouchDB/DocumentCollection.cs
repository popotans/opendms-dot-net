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

namespace Common.CouchDB
{
    /// <summary>
    /// A collection of CouchDB.Document objects
    /// </summary>
    public class DocumentCollection
    {
        /// <summary>
        /// The underlying list of Documents
        /// </summary>
        private ArrayList _documents;

        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentCollection()
        {
            _documents = new ArrayList();
        }

        /// <summary>
        /// Gets the quantity of Documents in the collection
        /// </summary>
        public int Count
        {
            get { return _documents.Count; }
        }

        /// <summary>
        /// Gets/Sets the Document at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>The CouchDB.Document corresponding to the index</returns>
        public Document this[int index]
        {
            get { return (Document)_documents[index]; }
            set { _documents[index] = value; }
        }

        /// <summary>
        /// Adds a file to the collection
        /// </summary>
        /// <param name="doc">The Document to add to the collection</param>
        public void Add(Document doc)
        {
            _documents.Add(doc);
        }

        /// <summary>
        /// Convert to an array of Documents
        /// </summary>
        /// <returns>An array of Documents</returns>
        public Document[] ToArray()
        {
            return (Document[])_documents.ToArray(typeof(Document));
        }
    }
}
