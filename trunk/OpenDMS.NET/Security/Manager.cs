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
using System.Collections.Generic;

namespace OpenDMS.Security
{
    internal class Manager
    {
        internal Manager()
        {
        }

        //internal Lease CheckLease(string keyspace, Guid guid)
        //{
        //    if (guid == Guid.Empty)
        //        throw new ArgumentNullException("guid");

        //    Lease lease = new Lease(_client);
        //    return lease.GetFromCassandra(keyspace, guid);
        //}

        //internal void CreateLease(string keyspace, Guid guid, string username, DateTime expiration)
        //{
        //    Lease lease = new Lease(_client) {  Guid = guid, 
        //                                        Username = username, 
        //                                        Leased = DateTime.Now, 
        //                                        Expiration = expiration };

        //    lease.SaveToCassandra(keyspace);
        //}

        //internal void ReleaseLease(string keyspace, Guid guid)
        //{
        //    Lease lease = CheckLease(keyspace, guid);

        //    if (lease != null)
        //        lease.DeleteFromCassandra(keyspace, guid);
        //}
    }
}
