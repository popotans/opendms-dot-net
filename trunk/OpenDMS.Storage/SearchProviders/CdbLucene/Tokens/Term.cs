using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public class Term : Token
    {
        public string Value { get; set; }

        public Term()
        {
        }

        public Term(string value)
        {
            Value = value;
        }

        public Term(string value, Operators.Base op)
            : this(value)
        {
            Operators.Add(op);
        }

        public Term(string value, Modifiers.Base mod)
            : this(value)
        {
            Modifiers.Add(mod);
        }

        public override string Stringify()
        {
            string output = "";

            if (Tokens.Count != 0)
                throw new QuerySyntaxException("A term must not have any tokens.");

            output = GetOperatorsAsString();
            output += Value;
            output = GetModifiersAsString(output);

            return output;
        }
    }
}
