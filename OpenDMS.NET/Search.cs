using System;
using System.Collections.Generic;

namespace OpenDMS
{
    public class Search
    {
        private string _queryString;
        private Storage.Master _storage;
        private string _requestingUser;

        public Search(string queryString, string requestingUser, Storage.Master storage)
        {
            _queryString = queryString;
            _requestingUser = requestingUser;
            _storage = storage;
        }

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
