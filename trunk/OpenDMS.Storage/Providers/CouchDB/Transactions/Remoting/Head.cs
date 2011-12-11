using System;
using Http = OpenDMS.Networking.Protocols.Http;
using Tcp = OpenDMS.Networking.Protocols.Tcp;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public class Head : Base
    {
        public string Id { get; protected set; }
        public string Rev { get; protected set; }

        public Head(IDatabase db, string id, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, id, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            Id = id;
        }

        public override void Process()
        {
            Commands.HeadDocument cmd;

            try
            {
                cmd = new Commands.HeadDocument(_uri);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the HeadDocument command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection, Commands.ReplyBase reply)
            {
                Rev = ((Commands.HeadDocumentReply)reply).Headers["Etag"];
                TriggerOnComplete(reply);
            };
            cmd.OnError += delegate(Commands.Base sender, Http.Client client, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            cmd.OnProgress += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection, Tcp.DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            cmd.OnTimeout += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection)
            {
                TriggerOnTimeout();
            };

            cmd.Execute(_sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize);
        }
    }
}
