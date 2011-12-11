using System;
using Http = OpenDMS.Networking.Protocols.Http;
using Tcp = OpenDMS.Networking.Protocols.Tcp;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public class GetView : Base
    {
        public Model.View View { get; protected set; }

        public GetView(IDatabase db, string designDocumentName, string viewName, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, "_design/" + designDocumentName + "/_view/" + viewName, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
        }

        public override void Process()
        {
            Commands.GetView cmd;

            try
            {
                cmd = new Commands.GetView(_uri);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the GetView command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection, Commands.ReplyBase reply)
            {
                View = ((Commands.GetViewReply)reply).View;
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
