using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene
{
    public class Query : IQuery
    {
        private List<IToken> _tokens;

        public Query()
        {
            _tokens = new List<IToken>();
        }

        public void Add(IToken token)
        {
            _tokens.Add(token);
        }

        public string Stringify()
        {
            string output = "";

            for (int i = 0; i < _tokens.Count; i++)
            {
                output += _tokens[i].ToString() + " ";
            }

            return output.Trim();
        }

        public override string ToString()
        {
            return Stringify();
        }
    }
}
