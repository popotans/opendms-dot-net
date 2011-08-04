using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class DetermineIfInstalled : Base
    {
        private int _count;
        private bool _onCompleteSent;

        public bool IsInstalled { get; private set; }

        public DetermineIfInstalled(IDatabase db, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _count = 0;
            _onCompleteSent = false;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "_design/groups", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "_design/users", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "globalusagerights", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "group-administrators", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
            RunTaskProcess(new Tasks.DetermineIfExists(_db, "user-administrator", _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));            
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.DetermineIfExists))
            {
                if (reply.IsError)
                {
                    _onCompleteSent = true;
                    IsInstalled = false;
                    TriggerOnComplete(reply, IsInstalled);
                    return;
                }

                _count++;
                if (_count >= 5 && !_onCompleteSent)
                {
                    _onCompleteSent = true;
                    IsInstalled = true;
                    TriggerOnComplete(null, IsInstalled);
                }
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
