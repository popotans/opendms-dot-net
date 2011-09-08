using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public class ResourceResult : SearchResultBase, IResourceResult
    {
        public List<string> Tags { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public string CheckedOutTo { get; set; }
        public DateTime LastCommit { get; set; }
        public string LastCommitter { get; set; }
        public string Title { get; set; }
        public List<Security.UsageRight> UsageRights { get; set; }

        public ResourceResult(string id, decimal score, DateTime created, string creator,
            DateTime modified, string modifier, DateTime? checkedoutat, string checkedoutto,
            DateTime lastcommit, string lastcommitter, string title, List<string> tags,
            List<Security.UsageRight> usagerights)
            : base(id, score, created, creator, modified, modifier)
        {
            CheckedOutAt = checkedoutat;
            CheckedOutTo = checkedoutto;
            LastCommit = lastcommit;
            LastCommitter = lastcommitter;
            Title = title;
            Tags = tags;
            UsageRights = usagerights;
        }

        public SearchProviders.ResourceResult MakeResource()
        {
            return new SearchProviders.ResourceResult(new Data.ResourceId(Id), null, null, null,
                    null, UsageRights)
                {
                    CheckedOutAt = CheckedOutAt,
                    CheckedOutTo = CheckedOutTo,
                    Created = Created,
                    Creator = Creator,
                    LastCommit = LastCommit,
                    LastCommitter = LastCommitter,
                    Modified = Modified,
                    Modifier = Modifier,
                    Tags = Tags,
                    Title = Title
                };
        }
    }
}
