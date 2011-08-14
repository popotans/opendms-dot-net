using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadGlobalUsageRights : Base
    {
        private IDatabase _db;
        private GlobalUsageRights _gur;

        public GlobalUsageRights GlobalUsageRights { get; private set; }

        public UploadGlobalUsageRights(IDatabase db, GlobalUsageRights globalUsageRights,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _gur = globalUsageRights;
        }

        public override void Process()
        {
            Remoting.SaveSingle rem;

            try
            {
                Transitions.GlobalUsageRights txGur = new Transitions.GlobalUsageRights();
                rem = new Remoting.SaveSingle(_db, txGur.Transition(_gur), _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.SaveSingle object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                if (!((Commands.PutDocumentReply)reply).Ok)
                    GlobalUsageRights = null;
                else
                {
                    GlobalUsageRights = new CouchDB.GlobalUsageRights(((Commands.PutDocumentReply)reply).Rev,
                        _gur.UsageRights);
                }
                TriggerOnComplete(reply);
            };
            rem.OnError += delegate(Remoting.Base sender, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            rem.OnProgress += delegate(Remoting.Base sender, OpenDMS.Networking.Http.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            rem.OnTimeout += delegate(Remoting.Base sender)
            {
                TriggerOnTimeout();
            };

            rem.Process();
        }
    }
}
