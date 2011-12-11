using System;
using Newtonsoft.Json.Linq;
using Http = OpenDMS.Networking.Protocols.Http;
using Tcp = OpenDMS.Networking.Protocols.Tcp;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public class SaveSingle : Save
    {
        public SaveSingle(IDatabase db, Model.Document input, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, input.Id, input, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
        }

        public override void Process()
        {
            Commands.PutDocument cmd;

            try
            {
                cmd = new Commands.PutDocument(_db, (Model.Document)_input);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutDocument command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection, Commands.ReplyBase reply)
            {
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
