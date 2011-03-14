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
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;

namespace Common.CouchDB.Lucene
{
    internal class SearchResultCollectionConverter : JavaScriptConverter
    {
        /// <summary>
        /// Gets the supported types
        /// </summary>
        public override System.Collections.Generic.IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(SearchResultCollection) })); }
        }

        /// <summary>
        /// Cannot be called
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="serializer">A reference to the JavaScriptSerializer</param>
        /// <returns>An IDictionary of strings and objects</returns>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes a dictionary of strings and objects into an object of the specified type
        /// </summary>
        /// <param name="dictionary">The dictionary to deserialize</param>
        /// <param name="type">The type of object to instantiate</param>
        /// <param name="serializer">A reference to the JavaScriptSerializer</param>
        /// <returns>The object of the specified type</returns>
        public override object Deserialize(System.Collections.Generic.IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type != typeof(SearchResultCollection))
                throw new ArgumentException("type must be a CouchDB.Lucene.ResultCollection.", "type");


            SearchResult res;
            SearchResultCollection rc = new SearchResultCollection();
            ArrayList itemsList;


            if (!dictionary.ContainsKey("rows"))
                throw new ArgumentException("dictionary does not contain a key entitled rows and must.", "dictionary");

            itemsList = (ArrayList)dictionary["rows"];

            for (int i = 0; i < itemsList.Count; i++)
            {
                res = SearchResult.Instantiate((Dictionary<string, object>)itemsList[i]);
                rc.Add(res);
            }

            return rc;
        }
    }
}
