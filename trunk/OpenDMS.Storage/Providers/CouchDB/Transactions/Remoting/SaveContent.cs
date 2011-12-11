using System;
using System.Collections.Generic;
using Http = OpenDMS.Networking.Protocols.Http;
using Tcp = OpenDMS.Networking.Protocols.Tcp;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Remoting
{
    public class SaveContent : Base
    {
        private Data.Version _version;

        public SaveContent(IDatabase db, Data.Version version, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, version.VersionId.ToString(), sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _version = version;
        }

        private OpenDMS.IO.FileStream GetStream()
        {
            OpenDMS.IO.File file = null;

            if (_version.Content.Stream == null)
            {
                if (string.IsNullOrEmpty(_version.Content.LocalFilepath))
                {
                    TriggerOnError("Data.Content has no source.", new NullReferenceException());
                    return null;
                }

                file = new OpenDMS.IO.File(_version.Content.LocalFilepath);
                if (!file.Exists())
                {
                    TriggerOnError("Data.Content has no source.", new NullReferenceException());
                    return null;
                }

                return file.GetStream(System.IO.FileMode.Open, System.IO.FileAccess.Read,
                    System.IO.FileShare.None, System.IO.FileOptions.None, _sendBufferSize, this);
            }
            else
            {
                return _version.Content.Stream;
            }
        }

        public override void Process()
        {
            Commands.PutAttachment cmd;            
            List<Exception> errors;
            OpenDMS.IO.FileStream stream;

            Transitions.Version txVersion = new Transitions.Version();
            Model.Document doc = txVersion.Transition(_version, out errors);

            if (errors != null)
            {
                TriggerOnError(errors[0].Message, errors[0]);
                return;
            }
                
            if (doc.Attachments == null || doc.Attachments.Count == 0)
            {
                TriggerOnError("No content to upload.", null);
                return;
            }
            else if(doc.Attachments.Count > 1)
            {
                TriggerOnError("To many content elements found, only one is expected.", null);
                return;
            }

            Dictionary<string, Model.Attachment>.Enumerator en = doc.Attachments.GetEnumerator();
            
            // Due to the above checks, we know there is 1 and only 1 entry
            en.MoveNext();
            stream = GetStream();

            if (stream == null)
            {
                TriggerOnError("Could not access a content stream.", null);
                return;
            }


            try
            {
                cmd = new Commands.PutAttachment(_db, doc, en.Current.Key, en.Current.Value, stream);
            }
            catch (Exception e)
            {
                Logger.Storage.Error("An exception occurred while creating the PutDocument command.", e);
                throw;
            }

            cmd.OnComplete += delegate(Commands.Base sender, Http.Client client, Http.HttpConnection connection, Commands.ReplyBase reply)
            {
                stream.Close();
                stream.Dispose();
                TriggerOnComplete(reply);
            };
            cmd.OnError += delegate(Commands.Base sender, Http.Client client, string message, Exception exception)
            {
                TriggerOnError(message, exception);
                stream.Close();
                stream.Dispose();
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
