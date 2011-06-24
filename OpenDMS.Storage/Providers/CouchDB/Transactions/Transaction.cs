using System;
using System.Collections.Generic;
using OpenDMS.IO;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions
{
    public class Transaction
    {
        public FileSystem FileSystem { get; private set; }
        public Directory Directory { get; private set; }
        public TimeSpan ExpiryDuration { get; private set; }

        public Transaction(FileSystem fileSystem, Directory directory, TimeSpan expiryDuration)
        {
            FileSystem = fileSystem;
            Directory = directory;
            ExpiryDuration = expiryDuration;
        }

        public void Begin(string username)
        {
            Lock loc;
            File file;

            // Does this transaction already exist?  Has it expired?
            if (Directory.Exists())
            {
                file = new File(Directory, "lock");

                // Does the lock exist?
                if (file.Exists())
                {
                    string errorMessage;
                    // Yes - load it
                    loc = Lock.Load(file);

                    // If the user can access then we just clear it out and relock
                    if (!loc.CanUserAccess(username, out errorMessage))
                        throw new ActiveTransactionException(errorMessage);
                    else
                    {
                        Reset();
                        Directory.Create();
                        loc.ResetExpiry();
                        loc.WriteLock(file);
                    }
                }
                else
                {
                    // Nope - clear out this transaction and create the lock
                    Reset();
                    Directory.Create();
                    loc = new Lock(username, ExpiryDuration);
                    loc.WriteLock(file);
                }                    
            }
        }

        public Stage GetCurrentStage(string username)
        {
            Lock loc;
            File file;
            string errorMessage;
            List<Directory> directories;
            int highNum = 0, temp;

            // Does this transaction already exist?  Has it expired?
            if (Directory.Exists())
            {
                file = new File(Directory, "lock");

                // Does the lock exist?
                if (!file.Exists())
                    throw new InvalidTransactionException("A transaction lock does not exist.");

                loc = Lock.Load(file);

                if (!loc.CanUserAccess(username, out errorMessage))
                    throw new ActiveTransactionException(errorMessage);
            }
            else
                throw new InvalidTransactionException("The transaction does not exist.");

            directories = Directory.GetDirectories();

            for (int i = 0; i < directories.Count; i++)
            {
                temp = int.Parse(directories[i].GetDirectoryShortName());
                if (temp > highNum)
                    highNum = temp;
            }

            // highNum now contains the # of the current stage.
            return new Stage(this, highNum);
        }

        public Stage AdvanceStage(string username)
        {
        }

        private void Reset()
        {
            if (Directory.Exists())
            {
                Directory.Delete();
            }
        }
    }
}
