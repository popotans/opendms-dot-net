using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public class SearchReply
    {
        public string Analyzer { get; set; }
        public string ETag { get; set; }
        public int FetchDuration { get; set; }
        public int Limit { get; set; }
        public string Plan { get; set; }
        public string Q { get; set; }
        public List<SearchResultBase> Results { get; set; }
        public int SearchDuration { get; set; }
        public int Skip { get; set; }
        public int TotalRows { get; set; }

        public SearchReply()
        {
            Results = new List<SearchResultBase>();
        }

        public SearchResult MakeResult()
        {
            List<SearchProviders.ResourceResult> resources = new List<SearchProviders.ResourceResult>();
            List<SearchProviders.VersionResult> versions = new List<SearchProviders.VersionResult>();

            for (int i=0; i<Results.Count; i++)
            {
                if (Results[i].GetType() == typeof(SearchProviders.CdbLucene.ResourceResult))
                    resources.Add(((SearchProviders.CdbLucene.ResourceResult)Results[i]).MakeResource());
                else if (Results[i].GetType() == typeof(SearchProviders.CdbLucene.VersionResult))
                    versions.Add(((SearchProviders.CdbLucene.VersionResult)Results[i]).MakeVersion());
                else
                    throw new UnsupportedException("Invalid type.");
            }

            return new SearchResult(resources, versions);
        }
    }
}
