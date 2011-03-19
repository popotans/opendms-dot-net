using System;
using Common.CouchDB;

namespace Common.Storage
{
    public class VersionManager
    {
        public static Resource GetCurrentResourceVersion(Guid guid, Database cdb)
        {
            Postgres.Resource dbRes = new Postgres.Resource(guid);
            Postgres.Version dbVer = dbRes.GetCurrentVersion();

            return new Resource(dbVer.VersionGuid, cdb);
        }
    }
}
