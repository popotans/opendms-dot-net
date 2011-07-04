using System;
using OpenDMS.IO;
using Newtonsoft.Json;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Stage
    {
        public delegate void CommitSuccessDelegate(Stage sender, Actions.Base action, ICommandReply reply);
        public delegate void CommitTimeoutDelegate(Stage sender, Actions.Base action, string message, Exception exception);
        public delegate void CommitErrorDelegate(Stage sender, Actions.Base action, string message, Exception exception);
        public delegate void CommitProgressDelegate(Stage sender, Actions.Base action, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public event CommitSuccessDelegate OnCommitSuccess;
        public event CommitTimeoutDelegate OnCommitTimeout;
        public event CommitErrorDelegate OnCommitError;
        public event CommitProgressDelegate OnCommitProgress;

        private int _number;
        private Directory _directory;
        private Transaction _transaction;
        private Model.Document _document;

        public Model.Document Document
        {
            get
            {
                if (_document == null)
                    _document = LoadDocument();
                return _document;
            }
        }

        public Stage(Transaction transaction, int number)
        {
            _transaction = transaction;
            _number = number;
            _directory = Directory.Append(transaction.Directory, number.ToString());
        }

        public void Execute(Actions.Base action)
        {
            _document = action.Execute();
            SaveDocument();
            _transaction.UpdateLockExpiry();
        }

        public void Delete()
        {
            _directory.Delete();
        }

        public void Commit(Actions.Base action)
        {
            action.OnTimeout += new Actions.Base.CommitTimeoutDelegate(Commit_OnTimeout);
            action.OnProgress += new Actions.Base.CommitProgressDelegate(Commit_OnProgress);
            action.OnError += new Actions.Base.CommitErrorDelegate(Commit_OnError);
            action.OnComplete += new Actions.Base.CommitCompletionDelegate(Commit_OnComplete);
            action.Commit();
        }

        private void Commit_OnComplete(Actions.Base sender, ICommandReply reply)
        {
            if (OnCommitSuccess != null) OnCommitSuccess(this, sender, reply);
            else throw new NotImplementedException("The OpenDMS.Storage.Providers.CouchDB.Transactions.Stage.OnSuccess event must be implemented.");
        }

        private void Commit_OnError(Actions.Base sender, string message, System.Exception exception)
        {
            if (OnCommitError != null) OnCommitError(this, sender, message, null);
            else throw new NotImplementedException("The OpenDMS.Storage.Providers.CouchDB.Transactions.Stage.OnCommitError event must be implemented.");
        }

        private void Commit_OnProgress(Actions.Base sender, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
        {
            if (OnCommitProgress != null) OnCommitProgress(this, sender, packetSize, sendPercentComplete, receivePercentComplete);
        }

        private void Commit_OnTimeout(Actions.Base sender)
        {
            if (OnCommitTimeout != null) OnCommitTimeout(this, sender, "The commit action timed out.", null);
            else throw new NotImplementedException("The OpenDMS.Storage.Providers.CouchDB.Transactions.Stage.OnCommitTimeout event must be implemented.");
        }

        public bool Exists()
        {
            return _directory.Exists();
        }

        private Model.Document LoadDocument()
        {
            JsonReader reader = null;

            try
            {
                File file = new File(_directory, "file.json");
                FileStream fs = file.GetStream(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, System.IO.FileOptions.None, 8192, this);
                System.IO.TextReader txtReader = new System.IO.StreamReader(fs);
                reader = new Newtonsoft.Json.JsonTextReader(txtReader);
                return new Model.Document(Model.Document.Load(reader));
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while loading the document.", e);
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        private void SaveDocument()
        {
            if (_document == null)
                throw new System.InvalidOperationException("The document has not been loaded.");

            JsonWriter writer = null;
            System.IO.TextWriter txtWriter;
            FileStream fs;
            File file;

            // Does the directory already exist?
            if (!_directory.Exists())
                _directory.Create();

            try
            {
                file = new File(_directory, "file.json");
                fs = file.GetStream(System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, System.IO.FileOptions.None, 8192, this);
                txtWriter = new System.IO.StreamWriter(fs);
                writer = new JsonTextWriter(txtWriter);
                _document.WriteTo(writer);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while saving the document.", e);
                throw;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
    }
}
