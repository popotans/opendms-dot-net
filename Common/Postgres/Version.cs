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

namespace Common.Postgres
{
    public class Version
    {
        public Guid ResourceGuid { get; set; }
        public UInt64 VersionNumber { get; set; }
        public Guid VersionGuid { get; set; }

        public Version(DataRow dr)
        {
            string resource_id, version_id, version_guid;

            // Make sure the fields exist and get their names
            resource_id = Database.DetermineColumnName(dr, "tbl_version.resource_id", "resource_id", "The row must contain a uuid value for 'resource_id'");
            version_id = Database.DetermineColumnName(dr, "tbl_version.version_id", "version_id", "The row must contain an integer value for 'version_id'");
            version_guid = Database.DetermineColumnName(dr, "tbl_version.version_guid", "version_guid", "The row must contain a uuid value for 'version_guid'");

            try { ResourceGuid = (Guid)dr[resource_id]; }
            catch { throw new Exception("The row must contain a uuid value for 'resource_id'"); }
            try { VersionNumber = (UInt64)dr[version_id]; }
            catch { throw new Exception("The row must contain an unsigned integer value for 'version_id'"); }
            try { VersionGuid = (Guid)dr[version_guid]; }
            catch { throw new Exception("The row must contain a uuid value for 'version_guid'"); }
        }
    }
}
