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

namespace Common.CouchDB.Lucene
{
    /// <summary>
    /// The SearchResultCollection class is a collection of SearchResults
    /// </summary>
    public class SearchResultCollection
    {
        /// <summary>
        /// An ArrayList of all the SearchResults
        /// </summary>
        private ArrayList _results;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResultCollection()
        {
            _results = new ArrayList();
        }

        /// <summary>
        /// Gets the Count property of _results - gets the number of results
        /// </summary>
        public int Count
        {
            get { return _results.Count; }
        }

        /// <summary>
        /// Gets/Sets the SearchResult at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SearchResult this[int index]
        {
            get { return (SearchResult)_results[index]; }
            set { _results[index] = value; }
        }

        /// <summary>
        /// Adds a SearchResult to the collection
        /// </summary>
        /// <param name="doc"></param>
        public void Add(SearchResult doc)
        {
            _results.Add(doc);
        }

        public SearchResult Get(string id)
        {
            for (int i = 0; i < _results.Count; i++)
            {
                if (((SearchResult)_results[i]).Id == id)
                    return (SearchResult)_results[i];
            }

            return null;
        }

        public SearchResult Get(Guid id)
        {
            return Get(id.ToString("N"));
        }

        /// <summary>
        /// Converts the collection to an array of SearchResult
        /// </summary>
        /// <returns>SearchResult array</returns>
        public SearchResult[] ToArray()
        {
            return (SearchResult[])_results.ToArray(typeof(SearchResult));
        }
    }
}
