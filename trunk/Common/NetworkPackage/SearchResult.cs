using System;
using System.Text;

namespace Common.NetworkPackage
{
    public class SearchResult 
        : ListBase<MetaAsset>
    {
        public SearchResult()
            : base("SearchResult")
        {
        }
    }
}
