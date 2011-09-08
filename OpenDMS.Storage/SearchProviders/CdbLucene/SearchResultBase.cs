using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public abstract class SearchResultBase
    {
        public string Id { get; set; }
        public decimal Score { get; set; }
        public DateTime Created { get; set; }
        public string Creator { get; set; }
        public DateTime Modified { get; set; }
        public string Modifier { get; set; }

        public SearchResultBase(string id, decimal score, DateTime created, string creator,
            DateTime modified, string modifier)
        {
            Id = id;
            Score = score;
            Created = created;
            Creator = creator;
            Modified = modified;
            Modifier = modifier;
        }
    }
}
