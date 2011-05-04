/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Common.CouchDB
{
    /// <summary>
    /// General utility type methods
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Determines the MimeType of the given filename or path based on the local system's registry
        /// </summary>
        /// <param name="Filename">The filename or filepath</param>
        /// <returns>A string containing the mime content type of the file</returns>
        public static string MimeType(string Filename)
        {
            string mime = "application/octetstream";
            string ext = System.IO.Path.GetExtension(Filename).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                mime = rk.GetValue("Content Type").ToString();
            return mime;
        }

        public static Uri BuildUri(Server server)
        {
            return new Uri("http://" + server.Host + ":" + server.Port);
        }

        public static Uri BuildUriForView(Database db, string designDoc, string viewName, string query)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/_design/" + designDoc + "/_view/" + viewName + "?" + query);
        }

        public static Uri BuildUriForSearch(Database db, string designDoc, string indexName, string query)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/_fti/_design/" + designDoc + "/" + indexName + "?" + query);
        }

        public static Uri BuildUriForDoc(Database db, string resource)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/" + resource);
        }

        public static Uri BuildUriForDoc(Database db, string resource, string revision)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/" + resource + "?rev=" + revision);
        }

        public static Uri BuildUriForAttachment(Database db, string resource, string attachmentName)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/" + resource + "/" + attachmentName);
        }

        public static Uri BuildUriForAttachment(Database db, string resource, string attachmentName, string revision)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/" + resource + "/" + attachmentName + "?rev=" + revision);
        }

        public static Uri BuildUriForAttachment(Database db, string resource, string attachmentName, int revision)
        {
            return new Uri("http://" + db.Server.Host + ":" + db.Server.Port + "/" + db.Name + "/" + resource + "/" + attachmentName + "?rev=" + revision.ToString());
        }
    }
}
