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
    public class Resource
    {
        public Guid Id { get; set; }
        public string LockedBy { get; set; }
        public DateTime? LockedAt { get; set; }

        public Resource(Guid id)
        {
            Id = id;
            LockedBy = null;
            LockedAt = null;
        }

        public Resource(DataRow dr)
        {
            string id, lockedby, lockedat;

            // Make sure the fields exist and get their names
            id = Database.DetermineColumnName(dr, "tbl_resource.id", "id", "The row must contain a uuid value for 'id'");
            lockedby = Database.DetermineColumnName(dr, "tbl_resource.lockedby", "lockedby", "The row must contain a string value for 'lockedby'");
            lockedat = Database.DetermineColumnName(dr, "tbl_resource.lockedat", "lockedat", "The row must contain a date/time value for 'lockedat'");

            try { Id = (Guid)dr[id]; }
            catch { throw new Exception("The row must contain a uuid value for 'id'"); }
            try 
            {
                if(dr[lockedby] != DBNull.Value)
                    LockedBy = (string)dr[lockedby]; 
            }
            catch { throw new Exception("The row must contain a string value for 'lockedby'"); }
            try
            {
                if (dr[lockedat] != DBNull.Value)
                    LockedAt = (DateTime?)dr[lockedat]; 
            }
            catch { throw new Exception("The row must contain a date/time value for 'lockedat'"); }
        }

        public static Resource Get(Guid id)
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            Resource r;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_resource WHERE id=:resourceid");
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = id;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            if (dt.Rows.Count <= 0)
            {
                Logger.General.Error("Could not find a resource with id " + id.ToString("N"));
                return null;
            }

            try
            {
                r = new Resource(dt.Rows[0]);
            }
            catch
            {
                Logger.General.Error("Could not parse a resource with id " + id.ToString("N"));
                return null;
            }

            return r;
        }

        public Version GetCurrentVersion()
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            Version v;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_version WHERE resource_id=:resourceid ORDER BY version_number DESC LIMIT 1");
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = Id;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            if (dt.Rows.Count <= 0)
            {
                Logger.General.Error("Could not find a version for resource id " + Id.ToString("N"));
                return null;
            }

            try
            {
                v = new Version(dt.Rows[0]);
            }
            catch
            {
                Logger.General.Error("Could not parse version for resource id " + Id.ToString("N"));
                return null;
            }

            return v;
        }

        public Version GetVersion(UInt64 versionNumber)
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            Version v;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_version WHERE resource_id=:resourceid AND version_number=:versionnumber");
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = Id;
            cmd.Parameters.Add(new NpgsqlParameter("versionnumber", NpgsqlTypes.NpgsqlDbType.Bigint));
            cmd.Parameters[1].Value = versionNumber;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            if (dt.Rows.Count <= 0)
            {
                Logger.General.Error("Could not find a version for resource id " + Id.ToString("N") + " with version number " + versionNumber.ToString());
                return null;
            }

            try
            {
                v = new Version(dt.Rows[0]);
            }
            catch
            {
                Logger.General.Error("Could not parse version for resource id " + Id.ToString("N") + " with version number " + versionNumber.ToString());
                return null;
            }

            return v;
        }

        public bool ApplyLock(string lockedby)
        {
            Database db;
            NpgsqlCommand cmd;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("UPDATE tbl_resource SET lockedby=:lockedby, lockedat=:lockedat WHERE id=:resourceid");
            cmd.Parameters.Add(new NpgsqlParameter("lockedby", NpgsqlTypes.NpgsqlDbType.Varchar, 100));
            cmd.Parameters[0].Value = lockedby;
            cmd.Parameters.Add(new NpgsqlParameter("lockedat", NpgsqlTypes.NpgsqlDbType.Timestamp));
            cmd.Parameters[1].Value = NowAsUtc;
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[2].Value = Id;


            try
            {
                db.Open();
                db.DBExec(cmd);
            }
            catch (Exception e)
            {
                Logger.General.Error("Could not apply lock on resource with id " + Id.ToString("N"), e);
                return false;
            }
            finally
            {
                db.Close();
            }

            return true;
        }

        public bool ReleaseLock()
        {
            Database db;
            NpgsqlCommand cmd;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("UPDATE tbl_resource SET lockedby=:lockedby, lockedat=:lockedat WHERE id=:resourceid");
            cmd.Parameters.Add(new NpgsqlParameter("lockedby", NpgsqlTypes.NpgsqlDbType.Varchar, 100));
            cmd.Parameters[0].Value = null;
            cmd.Parameters.Add(new NpgsqlParameter("lockedat", NpgsqlTypes.NpgsqlDbType.Timestamp));
            cmd.Parameters[1].Value = null;
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[2].Value = Id;

            try
            {
                db.Open();
                db.DBExec(cmd);
            }
            catch (Exception e)
            {
                Logger.General.Error("Could not release lock on resource with id " + Id.ToString("N"), e);
                return false;
            }
            finally
            {
                db.Close();
            }

            return true;
        }

        public Version CreateNewVersion()
        {
            Version version;

            if ((version = Version.CreateNewVersion(this)) == null)
                return null;

            return version;
        }

        public bool CreateInDatabase(out Version newVersion)
        {
            Database db;
            NpgsqlCommand cmd;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("INSERT INTO tbl_resource (id, lockedby, lockedat) VALUES (:a, :b, :c)");
            cmd.Parameters.Add(new NpgsqlParameter("a", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = Id;
            cmd.Parameters.Add(new NpgsqlParameter("b", NpgsqlTypes.NpgsqlDbType.Varchar, 50));
            cmd.Parameters[1].Value = LockedBy;
            cmd.Parameters.Add(new NpgsqlParameter("c", NpgsqlTypes.NpgsqlDbType.Timestamp));
            cmd.Parameters[2].Value = LockedAt;

            db.Open();
            db.DBExec(cmd);
            db.Close();

            newVersion = Version.CreateNewVersion(this);

            return true;
        }

        public System.Collections.Generic.List<Version> GetAllVersions()
        {
            Database db;
            DataTable dt;
            NpgsqlCommand cmd;
            System.Collections.Generic.List<Version> versions = new System.Collections.Generic.List<Version>();

            db = new Database(SettingsBase.Instance.PostgresConnectionString);

            cmd = new NpgsqlCommand("SELECT * FROM tbl_version WHERE resource_id=:resourceid");
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = Id;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            for(int i=0; i<dt.Rows.Count; i++)
            {
                versions.Add(new Version(dt.Rows[i]));
            }

            return versions;
        }

        public bool Delete()
        {
            Database db;
            NpgsqlCommand cmd1, cmd2;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);

            cmd1 = new NpgsqlCommand("DELETE FROM tbl_version WHERE resource_id=:resourceid");
            cmd1.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd1.Parameters[0].Value = Id;

            cmd2 = new NpgsqlCommand("DELETE FROM tbl_resource WHERE id=:resourceid");
            cmd2.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd2.Parameters[0].Value = Id;

            db.Open();
            db.DBExec(cmd1);
            db.DBExec(cmd2);
            db.Close();
            
            return true;
        }

        public static Resource GetResourceFromVersionId(Guid versionId)
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            Resource r;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_resource WHERE id IN (SELECT resource_id FROM tbl_version WHERE version_guid=:versionid)");
            cmd.Parameters.Add(new NpgsqlParameter("versionid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = versionId;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            if (dt.Rows.Count <= 0)
            {
                Logger.General.Error("Could not find a resource match for version id " + versionId.ToString("N"));
                return null;
            }

            try
            {
                r = new Resource(dt.Rows[0]);
            }
            catch
            {
                Logger.General.Error("Could not parse the resource match for version id " + versionId.ToString("N"));
                return null;
            }

            return r;
        }

        public static Resource CreateNewResource(string lockedBy, out Version newVersion)
        {
            Guid temp = Guid.Empty;
            Resource resource;
            bool loop = true;


            while (loop)
            {
                temp = Guid.NewGuid();
                loop = !TestResourceGuidForUniqueness(temp);
            }

            resource = new Resource(temp);

            if (!string.IsNullOrEmpty(lockedBy))
            {
                resource.LockedAt = NowAsUtc;
                resource.LockedBy = lockedBy;
            }

            resource.CreateInDatabase(out newVersion);
            return resource;
        }

        private static bool TestResourceGuidForUniqueness(Guid resourceGuid)
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt = null;

            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_resource WHERE id=:resourceGuid");
            cmd.Parameters.Add(new NpgsqlParameter("resourceGuid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = resourceGuid;

            db.Open();
            dt = db.GetTable(cmd);
            db.Close();

            return dt.Rows.Count == 0;
        }

        public static DateTime NowAsUtc
        {
            get 
            { 
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
                    DateTime.Now.Millisecond, DateTimeKind.Utc); 
            }
        }
    }
}
