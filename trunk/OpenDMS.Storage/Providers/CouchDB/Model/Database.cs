using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Model
{
    public class Database : BaseStorageObject
    {
        public bool CompactRunning
        {
            get { return this["compact_running"].Value<bool>(); }
            set { this["compact_running"] = value; }
        }

        public long CommittedUpdateSeq
        {
            get { return this["committed_update_seq"].Value<long>(); }
            set { this["committed_update_seq"] = value; }
        }

        public long DiskFormatVersion
        {
            get { return this["disk_format_version"].Value<long>(); }
            set { this["disk_format_version"] = value; }
        }

        public long DiskSize
        {
            get { return this["disk_size"].Value<long>(); }
            set { this["disk_size"] = value; }
        }

        public long DocCount
        {
            get { return this["disk_size"].Value<long>(); }
            set { this["disk_size"] = value; }
        }

        public long DocDelCount
        {
            get { return this["doc_del_count"].Value<long>(); }
            set { this["doc_del_count"] = value; }
        }

        public string DbName
        {
            get { return this["db_name"].Value<string>(); }
            set { this["db_name"] = value; }
        }

        public long InstanceStartTime
        {
            get { return this["instance_start_time"].Value<long>(); }
            set { this["instance_start_time"] = value; }
        }

        public long PurgeSeq
        {
            get { return this["purge_seq"].Value<long>(); }
            set { this["purge_seq"] = value; }
        }

        public long UpdateSeq
        {
            get { return this["purge_seq"].Value<long>(); }
            set { this["purge_seq"] = value; }
        }

        public Database()
        {
        }

        public Database(string json)
            : base(Parse(json))
        {
        }

        public Database(JToken token)
            : base((JObject)token)
        {
        }

        public Database(JObject jobj)
            : base()
        {
        }
    }
}