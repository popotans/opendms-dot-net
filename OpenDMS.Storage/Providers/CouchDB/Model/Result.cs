using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Data.Providers.CouchDB.Model
{
    public class Result<T> where T : ISerializable<T>
    {
    }
}
