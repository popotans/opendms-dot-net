using OpenDMS.Networking.Http.Methods;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class GetDatabaseReply : ReplyBase
    {
        private const string _200 = "Success.";
        private const string _404 = "The requested content could not be found.";

        public bool Ok { get; private set; }
        public bool CompactRunning { get; private set; }
        public ulong CommittedUpdateSeq { get; private set; }
        public int DiskFormatVersion { get; private set; }
        public ulong DiskSize { get; private set; }
        public ulong DocCount { get; private set; }
        public ulong DocDeleteCount { get; private set; }
        public string DbName { get; private set; }
        public ulong InstanceStartTime { get; private set; }
        public int PurgeSeq { get; private set; }
        public ulong UpdateSeq { get; private set; }

        public GetDatabaseReply(Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 201:
                    ResponseMessage = _200;
                    JObject jobj = JObject.Parse(StringifyResponseStream());
                    Ok = true;
                    CompactRunning = jobj["compact_running"].Value<bool>();
                    CommittedUpdateSeq = jobj["committed_update_seq"].Value<ulong>();
                    DiskFormatVersion = jobj["disk_format_version"].Value<int>();
                    DiskSize = jobj["disk_size"].Value<ulong>();
                    DocCount = jobj["doc_count"].Value<ulong>();
                    DocDeleteCount = jobj["doc_del_count"].Value<ulong>();
                    DbName = jobj["db_name"].Value<string>();
                    InstanceStartTime = jobj["instance_start_time"].Value<ulong>();
                    PurgeSeq = jobj["purge_seq"].Value<int>();
                    UpdateSeq = jobj["update_seq"].Value<ulong>();
                    break;
                case 404:
                    ResponseMessage = _404;
                    Ok = false;
                    break;
                default:
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
