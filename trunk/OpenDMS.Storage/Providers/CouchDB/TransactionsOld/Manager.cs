using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.TransactionsOld
{
    public class Manager : Singleton<Manager>
    {
        private Directory _directory;
        private LocalResourceLockManager _localResourceLockManager;

        public Manager()
        {
        }

        public void Initalize(Directory directory)
        {
            _directory = directory;
            _localResourceLockManager = new LocalResourceLockManager();
            _isInitialized = true;
        }

        public bool TransactionExists(string id)
        {
            return Directory.Append(_directory, id).Exists();
        }

        public bool TransactionIsActive(string id)
        {
            Lock loc;
            Transaction t;
            Directory dir = Directory.Append(_directory, id);

            if (!dir.Exists())
                return false;

            t = new Transaction(dir);
            loc = t.GetLock();
            if (loc == null)
                return false;
            return loc.IsExpired();
        }

        public bool UserAccessIsAuthorized(string username, string id, out string failureReason)
        {
            Lock loc;
            Transaction t;
            Directory dir = Directory.Append(_directory, id);

            if (!dir.Exists())
            {
                failureReason = "The transaction does not exist.";
                return false;
            }

            t = new Transaction(dir);
            loc = t.GetLock();
            return loc.CanUserAccess(username, out failureReason);
        }

        public Transaction CreateTransaction(string username, string id)
        {
            if (TransactionExists(id))
            {
                if (TransactionIsActive(id))
                    throw new InvalidTransactionException("The transaction already exists and is active.");
                else
                {
                    Transaction t;
                    Directory dir = Directory.Append(_directory, id);
                    t = new Transaction(dir);
                    t.Abort("System");
                }
            }
                    

            return new Transaction(Directory.Append(_directory, id));
        }

        public void AbortAllTransactions()
        {
            // Warning - this deletes all transactions without regard for ownership.
            // This will really throw a wrench into the works if used when any transactions are actively executing.
            _directory.Delete();
        }

        public bool GetLocalResourceLock(string id, string username, System.TimeSpan duration, out Lock currentLock)
        {
            return _localResourceLockManager.GetNewLock(id, username, duration, out currentLock);
        }

        public void ReleaseLock(string id)
        {
            _localResourceLockManager.ReleaseLock(id);
        }
    }
}
