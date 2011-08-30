using System;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public class Get : Base
    {
        public Model.Document Document { get; protected set; }

        public Get(IDatabase db, string id, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, id, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
        }

        public override void Process()
        {
            Commands.GetDocument cmd;

            try
            {
                cmd = new Commands.GetDocument(_uri);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetDocument command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Client client, Connection connection, Commands.ReplyBase reply)
            {
                Document = ((Commands.GetDocumentReply)reply).Document;
                TriggerOnComplete(reply);
            };
            cmd.OnError += delegate(Commands.Base sender, Client client, string message, Exception exception)
            {
                TriggerOnError(message, exception);
            };
            cmd.OnProgress += delegate(Commands.Base sender, Client client, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
            {
                TriggerOnProgress(direction, packetSize, sendPercentComplete, receivePercentComplete);
            };
            cmd.OnTimeout += delegate(Commands.Base sender, Client client, Connection connection)
            {
                TriggerOnTimeout();
            };

            cmd.Execute(_sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize);
        }
    }
}
