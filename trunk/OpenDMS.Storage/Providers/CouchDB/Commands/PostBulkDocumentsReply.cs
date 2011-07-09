using System.Collections.Generic;
using OpenDMS.Networking.Http.Methods;
using Newtonsoft.Json.Linq;

namespace OpenDMS.Storage.Providers.CouchDB.Commands
{
    public class PostBulkDocumentsReply : ReplyBase
    {
        public class Entry
        {
            public string Id { get; set; }
            public string Rev { get; set; }
            public string Error { get; set; }
            public string Reason { get; set; }
        }

        private const string _201 = "Document has been created successfully.";

        public bool AllOrNothing { get; set; }
        public List<Entry> Results { get; set; }

        public PostBulkDocumentsReply(Response response)
            : base(response)
        {
        }

        protected override void ParseResponse()
        {
            switch (_response.ResponseCode)
            {
                case 201:
                    Results = new List<Entry>();
                    Logger.Storage.Debug("Received a successful response from CouchDB.");
                    JArray jray = JArray.Parse(StringifyResponseStream());
                    for (int i = 0; i < jray.Count; i++)
                    {
                        JObject jobj2;
                        Entry entry = new Entry();
                        jobj2 = (JObject)jray[i];
                        entry.Id = jobj2["id"].Value<string>();
                        if (jobj2["rev"] != null) entry.Rev = jobj2["rev"].Value<string>();
                        if (jobj2["error"] != null) entry.Error = jobj2["error"].Value<string>();
                        if (jobj2["reason"] != null) entry.Reason = jobj2["reason"].Value<string>();
                        Results.Add(entry);
                    }
                    Logger.Storage.Debug("PostBulkDocumentsReply loaded.");
                    break;
                default:
                    Logger.Storage.Error("PostBulkDocumentsReply received an unknown response code: " + _response.ResponseCode.ToString());
                    throw new UnsupportedException("The response code " + _response.ResponseCode.ToString() + " is not supported.");
            }
        }
    }
}
