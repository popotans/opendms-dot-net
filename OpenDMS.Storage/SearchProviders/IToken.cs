using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders
{
    public interface IToken
    {
        List<IModifier> Modifiers { get; set; }
        List<IOperator> Operators { get; set; }
        List<IToken> Tokens { get; set; }
        string ToString();
    }
}
