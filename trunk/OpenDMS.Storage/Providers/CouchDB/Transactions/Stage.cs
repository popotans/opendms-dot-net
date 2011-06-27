using OpenDMS.IO;
using Newtonsoft.Json;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Stage
    {
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

        public bool Commit(Actions.Base action, out string errorMessage)
        {
            return action.Commit(out errorMessage);
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
