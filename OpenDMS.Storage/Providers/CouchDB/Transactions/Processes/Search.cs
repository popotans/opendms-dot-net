using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class Search : Base
    {
        private SearchArgs _args;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;

        public Search(IDatabase db, SearchArgs args,
            Security.RequestingPartyType requestingPartyType,
            Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _requestingPartyType = requestingPartyType;
            _session = session;
            _args = args;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.Search(_db, _args, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();

            if (t == typeof(Tasks.Search))
            {
                SearchProviders.CdbLucene.SearchReply searchReply;
                Tasks.Search task = (Tasks.Search)sender;
                Commands.GetDocumentReply r = (Commands.GetDocumentReply)reply;
                Transitions.SearchReply sr = new Transitions.SearchReply();
                searchReply = sr.Transition(task.Document);

                TriggerOnComplete(reply, searchReply.MakeResult());
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
