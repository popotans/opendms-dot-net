using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Transaction
    {
        private FileSystem _fileSystem;

        public Transaction(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        //public bool Begin(Data.Version version)
        //{
        //}
    }
}
