using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Tasks.Remoting
{
    public class SaveBulk : Save
    {
        public List<Commands.PostBulkDocumentsReply.Entry> Results { get; protected set; }

        public SaveBulk(IDatabase db, Model.BulkDocuments input)
            : base(db, null, input)
        {
        }

        public override void Process()
        {
            Commands.PostBulkDocuments cmd;

            try
            {
                cmd = new Commands.PostBulkDocuments(_db, (Model.BulkDocuments)_input);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PostBulkDocuments command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Client client, Connection connection, Commands.ReplyBase reply)
            {
                Results = ((Commands.PostBulkDocumentsReply)reply).Results;
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