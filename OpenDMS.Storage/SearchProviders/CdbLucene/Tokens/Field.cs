using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public class Field : Token
    {
        public string Name { get; set; }

        public Field()
        {
        }

        public Field(string name)
        {
            Name = name;
        }

        public override string Stringify()
        {
            return Name + ":" + base.Stringify();
        }
    }
}
