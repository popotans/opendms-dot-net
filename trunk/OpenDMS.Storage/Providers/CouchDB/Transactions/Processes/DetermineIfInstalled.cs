using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class DetermineIfInstalled : Base
    {
        private IDatabase _db;
        private int _count;
        private bool _onCompleteSent;

        public bool IsInstalled { get; private set; }

        public DetermineIfInstalled(IDatabase db)
        {
            _db = db;
            _count = 0;
            _onCompleteSent = false;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "_design/groups"));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "_design/users"));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "globalusagerights"));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "group-administrators"));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "user-administrator"));            
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DetermineIfExists))
            {
                _count++;
                if (_count >= 5 && !_onCompleteSent)
                {
                    _onCompleteSent = true;
                    TriggerOnComplete(null, null);
                }
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
