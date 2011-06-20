using System;

namespace OpenDMS.Storage.Providers.CouchDB
{
    public static class UriBuilder
    {
        public const string DEFAULT_PROTOCOL = "http";
        public const string DEFAULT_HOST = "127.0.0.1";
        public const int DEFAULT_PORT = 5984;

        public static Uri Build(IServer server)
        {
            return server.Uri;
        }

        public static Uri Build(IDatabase db)
        {
            return db.Uri;
        }

        public static Uri Build(IDatabase db, Data.ResourceId resourceId)
        {
            // http://<host>:<port>/<db>/<resource>
            return new Uri(db.Uri.ToString() + resourceId.ToString());
        }

        public static Uri Build(IDatabase db, Data.Resource resource)
        {
            return Build(db, resource.ResourceId);
        }

        public static Uri Build(IDatabase db, Data.VersionId versionId)
        {
            return new Uri(db.Uri.ToString() + versionId.ToString());
        }

        public static Uri Build(IDatabase db, Data.Version version)
        {
            return Build(db, version.VersionId);
        }

        public static Uri Build(IDatabase db, Model.Document doc)
        {
            return new Uri(db.Uri.ToString() + doc.Id);
        }

        public static Uri Build(IDatabase db, Model.Document doc, string attachmentName)
        {
            return new Uri(db.Uri.ToString() + doc.Id + "/" + attachmentName + "?rev=" + doc.Rev);
        }
    }
}
