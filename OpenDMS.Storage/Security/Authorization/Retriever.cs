using OpenDMS.Storage.Providers;

namespace OpenDMS.Storage.Security.Authorization
{
    public class Retriever
    {
        private IEngine _engine;
        private IDatabase _db;

        public Retriever(IEngine engine, IDatabase db)
        {
            _engine = engine;
            _db = db;
        }

        public void Execute()
        {
        }
    }
}
