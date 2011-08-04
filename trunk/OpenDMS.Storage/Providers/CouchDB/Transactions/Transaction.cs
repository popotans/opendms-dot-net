using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Transaction
    {
        //public delegate void ActionDelegate(Transaction sender, Processes.Base process, Tasks.Base task, EngineActionType actionType, bool willSendProgress);
        //public delegate void AuthorizationDelegate(Transaction sender, Processes.Base process, Tasks.Base task);
        //public delegate void CompletionDelegate(Transaction sender, Processes.Base process);
        //public delegate void ErrorDelegate(Transaction sender, Processes.Base process, Tasks.Base task, string message, Exception exception);
        //public delegate void ProgressDelegate(Transaction sender, Processes.Base process, Tasks.Base task, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        //public delegate void TimeoutDelegate(Transaction sender, Processes.Base process, Tasks.Base task);

        //public event ActionDelegate OnActionChanged;
        //public event AuthorizationDelegate OnAuthorizationDenied;
        //public event CompletionDelegate OnComplete;
        //public event ErrorDelegate OnError;
        //public event ProgressDelegate OnProgress;
        //public event TimeoutDelegate OnTimeout;

        private Processes.Base _process;

        public Transaction(Processes.Base process)
        {
            _process = process;
        }

        public static Transaction Create(Processes.Base process)
        {
            return new Transaction(process);
        }

        public Transaction Execute()
        {
            _process.Process();
            return this;
        }
    }
}
