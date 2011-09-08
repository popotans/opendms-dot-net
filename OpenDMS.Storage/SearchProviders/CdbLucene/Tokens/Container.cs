using System;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public abstract class Container : Token
    {
        protected string _open;
        protected string _close;

        public Container(char open, char close)
        {
            _open = open.ToString();
            _close = close.ToString();
        }

        public override string Stringify()
        {
            string output = "";

            output = GetOperatorsAsString();

            output += _open;

            for (int i = 0; i < Tokens.Count; i++)
            {
                output += Tokens[i].ToString() + " ";
            }

            output = output.Trim() + _close;

            output = GetModifiersAsString(output);

            return output;
        }
    }
}
