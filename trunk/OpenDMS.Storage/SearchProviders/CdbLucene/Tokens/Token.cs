using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.SearchProviders.CdbLucene.Tokens
{
    public abstract class Token : IToken
    {
        public List<IOperator> Operators { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public List<IToken> Tokens { get; set; }

        public Token()
        {
            Tokens = new List<IToken>();
            Operators = new List<IOperator>();
            Modifiers = new List<IModifier>();
        }

        public virtual string Stringify()
        {
            string output = "";

            output = GetOperatorsAsString();

            //if (Tokens.Count > 1)
            //    output += "(";

            for (int i = 0; i < Tokens.Count; i++)
            {
                output += Tokens[i].ToString() + " ";
            }

            //if (Tokens.Count > 1)
            //{
            //    output = output.Trim();
            //    output += ")";
            //}

            output = GetModifiersAsString(output);

            return output;
        }

        protected string GetOperatorsAsString()
        {
            string output = "";

            for (int i = 0; i < Operators.Count; i++)
            {
                output += Operators[i].ToString() + " ";
            }

            return output;
        }

        protected string GetOperatorsAsString(string attachTo)
        {
            return attachTo.Trim() + GetOperatorsAsString();
        }

        protected string GetModifiersAsString()
        {
            string output = "";

            for (int i = 0; i < Modifiers.Count; i++)
            {
                output += Modifiers[i].ToString() + " ";
            }

            return output;
        }

        protected string GetModifiersAsString(string attachTo)
        {
            return attachTo.Trim() + GetModifiersAsString();
        }

        public override string ToString()
        {
            return Stringify();
        }
    }
}
