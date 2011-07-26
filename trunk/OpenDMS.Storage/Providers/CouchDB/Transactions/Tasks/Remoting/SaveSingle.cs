using System;
using Newtonsoft.Json.Linq;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks.Remoting
{
    public class SaveSingle : Save
    {
        public SaveSingle(IDatabase db, Model.Document input)
            : base(db, input.Id, input)
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

            cmd.OnComplete += delegate(Commands.Base sender, Client client, Connection connection, Commands.ReplyBase reply)
            {
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
        }
    }
}
