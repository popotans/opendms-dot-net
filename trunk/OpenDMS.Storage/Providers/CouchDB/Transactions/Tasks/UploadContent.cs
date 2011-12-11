using System;
using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks
{
    public class UploadContent : Base
    {
        private IDatabase _db;
        private Data.Version _version;

        public Data.Version Version { get; private set; }
        public Data.Content Content { get; private set; }

        public UploadContent(IDatabase db, Data.Version version,
            int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _db = db;
            _version = version;
        }

        public override void Process()
        {
            Remoting.SaveContent rem;

            try
            {
                rem = new Remoting.SaveContent(_db, _version, _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while instantiating the Transactions.Tasks.Remoting.SaveContent object.", e);
                throw;
            }

            rem.OnComplete += delegate(Remoting.Base sender, ICommandReply reply)
            {
                if (!((Commands.PutAttachmentReply)reply).Ok)
                {
                    Version = null;
                    Content = null;
                }
                else
                {
                    Version = _version;
                    Content = _version.Content;
                    Version.UpdateRevision(((Commands.PutAttachmentReply)reply).Rev);
                }
                TriggerOnComplete(reply);
            };
            rem.OnError += delegate(Remoting.Base sender, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            rem.OnProgress += delegate(Remoting.Base sender, OpenDMS.Networking.Protocols.Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
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
