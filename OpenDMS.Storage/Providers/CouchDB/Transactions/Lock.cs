using System;
using OpenDMS.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    internal class Lock
    {
        private string _owningUsername;
        private DateTime _created;
        private DateTime _expiry;
        private TimeSpan _duration;

        public Lock(string owningUsername, TimeSpan duration)
        {
            _owningUsername = owningUsername;
            _created = DateTime.Now;
            _duration = duration;
            _expiry = _created + _duration;
        }

        public Lock(string owningUsername, DateTime created, TimeSpan duration)
        {
            _owningUsername = owningUsername;
            _created = created;
            _duration = duration;
            _expiry = _created + _duration;
        }

        public Lock(string owningUsername, DateTime created, TimeSpan duration, DateTime expiry)
        {
            _owningUsername = owningUsername;
            _created = created;
            _duration = duration;
            _expiry = expiry;
        }

        public void WriteLock(File file)
        {
            JObject jobj = new JObject();
            JsonWriter writer;
            System.IO.TextWriter txtWriter;
            FileStream fs;

            jobj.Add("OwningUsername", _owningUsername);
            jobj.Add("Created", _created);
            jobj.Add("Duration", _duration.ToString("G"));
            jobj.Add("Expiry", _expiry);

            fs = file.GetStream(System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, System.IO.FileOptions.None, 8192, this);
            txtWriter = new System.IO.StreamWriter(fs);
            writer = new JsonTextWriter(txtWriter);
            jobj.WriteTo(writer);
        }

        public static Lock Load(File file)
        {
            string owningUsername;
            DateTime created, expiry;
            TimeSpan duration;

            FileStream fs = file.GetStream(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None, System.IO.FileOptions.None, 8192, this);
            System.IO.TextReader txtReader = new System.IO.StreamReader(fs);
            JsonReader reader = new Newtonsoft.Json.JsonTextReader(txtReader);
            JObject jobj = JObject.Load(reader);

            owningUsername = jobj["OwningUsername"].Value<string>();
            created = jobj["Created"].Value<DateTime>();
            duration = TimeSpan.Parse(jobj["Duration"].Value<string>());
            expiry = jobj["Expiry"].Value<DateTime>();

            return new Lock(owningUsername, created, duration, expiry);
        }

        public bool CanUserAccess(string username, out string errorMessage)
        {
            if (_owningUsername != username)
            {
                errorMessage = "Invalid user.";
                return false;
            }

            if (_created + _duration < DateTime.Now)
            {
                errorMessage = "The transaction timed out.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public void ResetExpiry()
        {
            _expiry = DateTime.Now + _duration;
        }
    }
}
