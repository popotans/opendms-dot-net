using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public class VersionResult : SearchResultBase, IVersionResult
    {
        public string Md5 { get; set; }
        public string Extension { get; set; }

        public VersionResult(string id, decimal score, DateTime created, string creator,
            DateTime modified, string modifier, string md5, string extension)
            : base(id, score, created, creator, modified, modifier)
        {
            Md5 = md5;
            Extension = extension;
        }

        public SearchProviders.VersionResult MakeVersion()
        {
            return new SearchProviders.VersionResult(new Data.VersionId(Id), null, null, null)
            {
                Created = Created,
                Creator = Creator,
                Modified = Modified,
                Modifier = Modifier,
                Extension = Extension,
                Md5 = Md5
            };
        }
    }
}
