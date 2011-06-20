using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Manager : Singleton<Manager>
    {
        public Manager()
        {
        }

        public void Initalize()
        {
            _isInitialized = true;
        }
    }
}
