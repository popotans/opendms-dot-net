using System;

namespace Common.Work
{
    public class JobArgs
    {
        public IWorkRequestor Requestor { get; set; }
        public ulong Id { get; set; }
        public Storage.Version Resource { get; set; }
        public JobBase.UpdateUIDelegate UpdateUICallback { get; set; }
        public uint Timeout { get; set; }
        public string RequestingUser { get; set; }
        public ErrorManager ErrorManager { get; set; }
        public FileSystem.IO FileSystem { get; set; }
        public CouchDB.Database CouchDB { get; set; }
        public JobBase.ProgressMethodType ProgressMethod { get; set; }
        public Master.JobType JobType { get; set; }
    }
}
