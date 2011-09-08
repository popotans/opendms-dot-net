using System;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IModifier
    {
        string Argument { get; }
        string Key { get; }
        string ToString();
    }
}
