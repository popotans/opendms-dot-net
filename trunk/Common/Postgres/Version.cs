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
using System.Data;
using Npgsql;

namespace Common.Postgres
{
    public class Version
    {
        public Guid ResourceGuid { get; set; }
        public Int64 VersionNumber { get; set; }
        public Guid VersionGuid { get; set; }

        public Version(Guid resourceId)
        {
            ResourceGuid = resourceId;
        }

        public Version(DataRow dr)
        {
            string resource_id, version_number, version_guid;

            // Make sure the fields exist and get their names
            resource_id = Database.DetermineColumnName(dr, "tbl_version.resource_id", "resource_id", "The row must contain a uuid value for 'resource_id'");
            version_number = Database.DetermineColumnName(dr, "tbl_version.version_number", "version_number", "The row must contain an integer value for 'version_number'");
            version_guid = Database.DetermineColumnName(dr, "tbl_version.version_guid", "version_guid", "The row must contain a uuid value for 'version_guid'");

            try { ResourceGuid = (Guid)dr[resource_id]; }
            catch { throw new Exception("The row must contain a uuid value for 'resource_id'"); }
            try { VersionNumber = (Int64)dr[version_number]; }
            catch { throw new Exception("The row must contain an unsigned integer value for 'version_number'"); }
            try { VersionGuid = (Guid)dr[version_guid]; }
            catch { throw new Exception("The row must contain a uuid value for 'version_guid'"); }
        }

        public bool CreateInDatabase()
        {
            Database db;
            NpgsqlCommand cmd;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("INSERT INTO tbl_version (resource_id, version_number, version_guid) VALUES (:a, :b, :c)");
            cmd.Parameters.Add(new NpgsqlParameter("a", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = ResourceGuid;
            cmd.Parameters.Add(new NpgsqlParameter("b", NpgsqlTypes.NpgsqlDbType.Bigint));
            cmd.Parameters[1].Value = VersionNumber;
            cmd.Parameters.Add(new NpgsqlParameter("c", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[2].Value = VersionGuid;

            db.Open();
            db.DBExec(cmd);
            db.Close();

            return true;
        }

        public static Version CreateNewVersion(Resource resource)
        {
            bool loop = true;
            Version currentVersion;


            currentVersion = resource.GetCurrentVersion();

            if (currentVersion == null)
            {
                currentVersion = new Version(resource.Id)
                {
                    VersionNumber = 1,
                    VersionGuid = Guid.NewGuid()
                };
            }
            else
                currentVersion.VersionNumber++;

            // Find an unused guid
            while (loop)
            {
                currentVersion.VersionGuid = Guid.NewGuid();
                loop = !TestVersionGuidForUniqueness(currentVersion.VersionGuid);
            }

            // After the loop exists, we have a currentVersion with a new unique version_guid.
            if (!currentVersion.CreateInDatabase())
                return null;

            return currentVersion;
        }

        private static bool TestVersionGuidForUniqueness(Guid versionGuid)
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            
            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_version WHERE version_guid=:newguid");
            cmd.Parameters.Add(new NpgsqlParameter("newguid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = versionGuid;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            return dt.Rows.Count == 0;
        }
    }
}
