using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public class Phrase : Container
    {
        public Phrase()
            : base('\"', '\"')
        {
        }
    }
}
