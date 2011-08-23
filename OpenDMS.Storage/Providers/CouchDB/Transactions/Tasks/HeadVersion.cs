using System;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class HeadVersion : Base
    {
        private IDatabase _db;
        private Data.VersionId _versionId;

        public Data.VersionId VersionId { get; private set; }
        public string Revision { get; private set; }

        public HeadVersion(IDatabase db, Data.VersionId versionId,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _versionId = versionId;
        }

        public override void Process()
        {
            Remoting.Head rem;

            try
            {
                rem = new Remoting.Head(_db, _versionId.ToString(), _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.Head object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                VersionId = _versionId;
                Revision = ((Remoting.Head)sender).Rev.Trim(new char[] { '\"' });
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
