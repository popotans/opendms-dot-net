using System;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IOperator
    {
        string Key { get; }
        string ToString();
    }
}
