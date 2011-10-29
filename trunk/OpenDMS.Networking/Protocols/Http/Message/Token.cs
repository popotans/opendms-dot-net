using System;
using System.Collections.Generic;

namespace OpenDMS.Networking.Protocols.Http.Message
{
    public class Token
    {
        private string _value;
        private List<string> _seperators;

        public string Value
        {
            get { return _value; }
            set
            {
                for (byte i=0; i<=25; i++)
                {
                    if (StringContainsCharacter(value, i))
                        throw new InvalidTokenException("Token cannot have a value containing ASCII character code " + i.ToString());
                }
                if (StringContainsCharacter(value, 127)) // DEL
                    throw new InvalidTokenException("Token cannot have a value containing ASCII character code 127");
                for (int i = 0; i < _seperators.Count; i++)
                {
                    if (StringContainsCharacter(value, char.Parse(_seperators[i])))
                        throw new InvalidTokenException("Token cannot have a value containing ASCII character code " + char.Parse(_seperators[i]));
                }
                _value = value;
            }
        }

        private Token()
        {
            BuildSeperatorsList();
        }

        public Token(string value)
            : this()
        {
            Value = value;
        }

        private void BuildSeperatorsList()
        {
            _seperators = new List<string>();
            
            _seperators.Add("(");
            _seperators.Add(")");
            _seperators.Add("<");
            _seperators.Add(">");
            _seperators.Add("@");
            _seperators.Add(",");
            _seperators.Add(";");
            _seperators.Add(":");
            _seperators.Add("\\");
            _seperators.Add("\"");
            _seperators.Add("/");
            _seperators.Add("[");
            _seperators.Add("]");
            _seperators.Add("?");
            _seperators.Add("=");
            _seperators.Add("{");
            _seperators.Add("}");
            _seperators.Add(" ");
            _seperators.Add(char.ToString((char)34));
        }

        private bool StringContainsCharacter(string str, byte asciiCharacterCode)
        {
            byte[] bytes = new byte[1] { (byte)asciiCharacterCode };
            return str.Contains(System.Text.Encoding.ASCII.GetString(bytes));
        }
        private bool StringContainsCharacter(string str, char asciiCharacterCode)
        {
            return StringContainsCharacter(str, (byte)asciiCharacterCode);
        }
    }
}
