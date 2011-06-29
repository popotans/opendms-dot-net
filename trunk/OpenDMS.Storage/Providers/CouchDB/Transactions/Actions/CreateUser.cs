

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Actions
{
    public class CreateUser : Base
    {
        private IDatabase _db;

        public CreateUser(IDatabase db, Model.Document doc)
            : base(doc)
        {
            _db = db;
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

        private void Commit_OnTimeout(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection)
        {
            TriggerOnTimeout();
        }

        private void Commit_OnError(Commands.Base sender, Networking.Http.Client client, string message, System.Exception exception)
        {
            TriggerOnError(message, exception);
        }

        private void Commit_OnComplete(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Commands.ReplyBase reply)
        {
            TriggerOnComplete(reply);
        }

        private void Commit_OnProgress(Commands.Base sender, Networking.Http.Client client, Networking.Http.Connection connection, Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            TriggerOnProgress(packetSize, sendPercentComplete, receivePercentComplete);
        }
    }
}
