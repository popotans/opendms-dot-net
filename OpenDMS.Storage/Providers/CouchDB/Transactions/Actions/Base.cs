
namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Actions
{
    public abstract class Base
    {
        protected Model.Document _document;

        public abstract Model.Document Execute();
        public abstract bool Commit(out string errorMessage);

        public Base(Model.Document document)
        {
            _document = document;
        }
    }
}
