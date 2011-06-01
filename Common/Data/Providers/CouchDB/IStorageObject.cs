using System;
using Common.Http.Methods;
using Common.Data.Providers.CouchDB.Commands;

namespace Common.Data.Providers.CouchDB
{
    public interface IStorageObject
    {
        Uri Uri { get; }
        List List { get; }
        Create Create { get; }
        Update Update { get; }
        Delete Delete { get; }
        Get Get { get; }
    }
}
