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

namespace HttpModule
{
    /// <summary>
    /// Provides an interface for searching using Solr
    /// </summary>
    public class Search
    {
        /// <summary>
        /// The query represented as a string.
        /// </summary>
        private string _queryString;
        /// <summary>
        /// The <see cref="Storage.Master"/> providing a storage facility.
        /// </summary>
        private Storage.Master _storage;
        /// <summary>
        /// The user requesting the search.
        /// </summary>
        private string _requestingUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Search"/> class.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="requestingUser">The requesting user.</param>
        /// <param name="storage">The <see cref="Storage.Master"/> providing a storage facility.</param>
        public Search(string queryString, string requestingUser, Storage.Master storage)
        {
            _queryString = queryString;
            _requestingUser = requestingUser;
            _storage = storage;
        }

        /// <summary>
        /// Executes the search.
        /// </summary>
        /// <param name="generalLogger">A reference to the <see cref="Common.Logger"/> that this instance should use to document general events.</param>
        /// <param name="networkLogger">A reference to the <see cref="Common.Logger"/> that this instance should use to document network events.</param>
        /// <param name="response">The response from the Solr server.</param>
        /// <returns>The results of the search.</returns>
        public Common.NetworkPackage.SearchResult Execute(Common.Logger generalLogger, Common.Logger networkLogger, 
            out Common.NetworkPackage.ServerResponse response)
        {
            string errorMessage;
            Common.Data.MetaAsset ma;
            Common.NetworkPackage.MetaAsset netMa;
            Common.NetworkPackage.SearchResult retVal = new Common.NetworkPackage.SearchResult();
            List<SolrResult> results;

            SolrSearch search = new SolrSearch(Settings.Instance.SearchHost.Address.ToString(),
                Settings.Instance.SearchHost.Port);

            results = search.Execute(_queryString, generalLogger, networkLogger, out response);

            if (response != null)
                return null;

            for (int i = 0; i < results.Count; i++)
            {
                if (_storage.GetMeta(results[i].Id, _requestingUser, true, out ma, out errorMessage) ==
                    Storage.ResultType.Success)
                {
                    netMa = ma.ExportToNetworkRepresentation();
                    retVal.Add(netMa);
                }
            }

            return retVal;
        }
    }
}
