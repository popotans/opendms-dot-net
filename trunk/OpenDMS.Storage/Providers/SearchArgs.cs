using System;

namespace OpenDMS.Storage.Providers
{
    public class SearchArgs
    {
        public SearchProviders.IQuery Query { get; set; }

        public SearchArgs(SearchProviders.IQuery query)
        {
            Query = query;
        }
    }
}
