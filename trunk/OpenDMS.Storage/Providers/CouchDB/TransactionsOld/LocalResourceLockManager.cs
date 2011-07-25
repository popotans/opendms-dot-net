using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.TransactionsOld
{
    public class LocalResourceLockManager
    {
        public Dictionary<string, Lock> _locks;

        public LocalResourceLockManager()
        {
            _locks = new Dictionary<string, Lock>();
        }

        public bool GetNewLock(string id, string owningUsername, TimeSpan duration, out Lock currentLock)
        {
            lock (_locks)
            {
                currentLock = null;

                if (_locks.ContainsKey(id))
                {
                    currentLock = _locks[id];
                    return false;
                }

                _locks.Add(id, new Lock(owningUsername, duration));
                return true;
            }
        }

        public void ReleaseLock(string id)
        {
            lock (_locks)
            {
                if (!_locks.ContainsKey(id))
                    throw new ArgumentOutOfRangeException("The argument does not exist");

                _locks.Remove(id);
            }
        }
    }
}
