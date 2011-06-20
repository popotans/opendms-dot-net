using System;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Json
{
    public class ValueParser
    {
        public static object Parse(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.None:
                    return token.Value<object>();
                case JTokenType.Object:
                    return token.Value<object>();
                case JTokenType.Array:
                    throw new InvalidCastException("Array not supported.");
                case JTokenType.Constructor:
                    throw new InvalidCastException("Constructor not supported.");
                case JTokenType.Property:
                    throw new InvalidCastException("Property not supported.");
                case JTokenType.Comment:
                    throw new InvalidCastException("Comment not supported.");
                case JTokenType.Integer:
                    return token.Value<long>();
                case JTokenType.Float:
                    return token.Value<float>();
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Null:
                    return null;
                case JTokenType.Undefined:
                    throw new InvalidCastException("Undefined not supported.");
                case JTokenType.Date:
                    return token.Value<DateTime>();
                case JTokenType.Raw:
                    throw new InvalidCastException("Raw not supported.");
                case JTokenType.Bytes:
                    return token.Value<byte[]>();
                default:
                    throw new UnsupportedException("Unknown token type.");
            }
        }
    }
}
