using System;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IQuery
    {
        void Add(IToken token);
        string ToString();
    }
}
