using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public class Group : Container
    {
        public Group()
            : base('(', ')')
        {
        }
    }
}
