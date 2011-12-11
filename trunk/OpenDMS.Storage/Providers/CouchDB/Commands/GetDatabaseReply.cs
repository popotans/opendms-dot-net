using Http = OpenDMS.Networking.Protocols.Http;
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

        public GetDatabaseReply(Http.Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.StatusLine.StatusCode)
            {
                case 201:
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
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
                    Logger.Storage.Debug("GetDatabaseReply loaded.");
                    break;
                case 404:
                    Logger.Storage.Debug("Received a failure response from CouchDB: " + _404);
                    ResponseMessage = _404;
                    Ok = false;
                    Logger.Storage.Debug("GetDatabaseReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("GetDatabaseReply received an unknown response code: " + _response.StatusLine.StatusCode.ToString());
                    Ok = false;
                    throw new UnsupportedException("The response code " + _response.StatusLine.StatusCode.ToString() + " is not supported.");
            }
        }
    }
}
