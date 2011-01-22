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
