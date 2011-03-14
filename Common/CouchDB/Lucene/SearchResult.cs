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
using System.Linq;
using System.Text;

namespace Common.CouchDB.Lucene
{
    /// <summary>
    /// The Search Result class is used to represent a result of a search against a CouchDB-Lucene provider
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// The CouchDB Document _id
        /// </summary>
        private string _id;

        /// <summary>
        /// Gets the Document _id
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The CouchDB Document _id</param>
        public SearchResult(string id)
        {
            _id = id;
            Logger.General.Debug("Common.CouchDB.Lucene.SearchResult instantiated.");
        }

        /// <summary>
        /// Creates a new instance of a CouchDB.Lucene.SearchResult
        /// </summary>
        /// <param name="dictionary">Dictionary to iterate for property values</param>
        /// <returns>A new CouchDB.Lucene.SearchResult instance</returns>
        public static SearchResult Instantiate(Dictionary<string, object> dictionary)
        {
            // Make sure the keys exist
            if (!dictionary.ContainsKey("id"))
            {
                throw new ArgumentException("dictionary must contain valid keys for: id", "dictionary");
            }

            // Make sure their values exist and are strings
            try
            {
                if (string.IsNullOrEmpty((string)dictionary["id"]))
                {
                    throw new ArgumentException("dictionary must contain non-null and non-empty keys for: id.", "dictionary");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("dictionary must contain string values for keys: id.", "dictionary", e);
            }

            SearchResult sr;

            // Create the new SearchResult instance
            sr = new SearchResult((string)dictionary["id"]);

            return sr;
        }
    }
}
