using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Modifiers
{
    public abstract class Base : IModifier
    {
        public string Key { get; private set; }
        public string Argument { get; private set; }

        public Base(string key)
        {
            Key = key;
        }

        public Base(string key, string argument)
            : this(key)
        {
            Argument = argument;
        }

        public abstract string Stringify();

        public override string ToString()
        {
            return Stringify();
        }
    }
}
