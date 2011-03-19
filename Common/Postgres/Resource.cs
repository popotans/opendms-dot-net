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

        public Resource(Guid id)
        {
            Id = id;
        }

        public Version GetCurrentVersion()
        {
            Database db;
            NpgsqlCommand cmd;
            DataTable dt;
            Version v;
            
            db = new Database(SettingsBase.Instance.PostgresConnectionString);
            cmd = new NpgsqlCommand("SELECT * FROM tbl_version WHERE resource_id=:resourceid LIMIT 1");
            cmd.Parameters.Add(new NpgsqlParameter("resourceid", NpgsqlTypes.NpgsqlDbType.Uuid));
            cmd.Parameters[0].Value = Id;

            dt = db.GetTable(cmd);

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
    }
}
