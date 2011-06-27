using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Manager : Singleton<Manager>
    {
        private Directory _directory;

        public Manager()
        {
        }

        public void Initalize(Directory directory)
        {
            _directory = directory;
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
                throw new InvalidTransactionException("The transaction already exists.");

            return new Transaction(Directory.Append(_directory, id));
        }
    }
}
