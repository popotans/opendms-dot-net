using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Operators
{
    public abstract class Base : Tokens.Token, IOperator
    {
        public string Key { get; private set; }

        public Base(string key)
        {
            Key = key;
        }

        public override string Stringify()
        {
            return Key;
        }
    }
}
