
namespace OpenDMS.Storage.Providers.CouchDB.TransactionsOld.Actions
{
    public class CreateUser : Base
    {
        public CreateUser(IDatabase db, Model.Document doc)
            : base(db, doc)
        {
        }

        public override Model.Document Execute()
        {
            return _document;
        }

        public override void Commit()
        {
            Commands.PutDocument cmd;

            cmd = new Commands.PutDocument(_db, _document);

            cmd.OnProgress += new Commands.Base.ProgressDelegate(Commit_OnProgress);
            cmd.OnComplete += new Commands.Base.CompletionDelegate(Commit_OnComplete);
            cmd.OnError += new Commands.Base.ErrorDelegate(Commit_OnError);
            cmd.OnTimeout += new Commands.Base.TimeoutDelegate(Commit_OnTimeout);

            cmd.Execute(_db.Server.Timeout, _db.Server.Timeout, _db.Server.BufferSize, _db.Server.BufferSize);
        }
    }
}
