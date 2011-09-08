using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public class Range : Container
    {
        public Range()
            : base('[', ']')
        {
        }

        public Range(string from, string to)
            : base('[', ']')
        {
            Tokens.Add(new Term(from));
            Tokens.Add(new Term(to));
        }

        public override string Stringify()
        {
            string output = "";
            
            if (Tokens.Count != 2)
                throw new QuerySyntaxException("A range must have two and only two tokens.");

            output = GetOperatorsAsString();

            output += _open + Tokens[0].ToString() + " TO " + Tokens[1].ToString() + _close;

            output = GetModifiersAsString(output);

            return output;
        }
    }
}
