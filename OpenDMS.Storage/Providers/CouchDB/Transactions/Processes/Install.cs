using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class Install : Base
    {
        // Installation package, administrator user password is 'password'.
        private string _package =
@"{ 
    ""docs"" : [
        {
            ""_id"" : ""_design/groups"",
            ""language"": ""javascript"",
            ""views"": {
                ""GetAll"": {
                    ""map"": ""function(doc) { if (doc.Type == \""group\"") { emit(doc); } }""
                }
            }
        },
        {
            ""_id"": ""_design/users"",
            ""language"": ""javascript"",
            ""views"": {
                ""GetAll"": {
                    ""map"": ""function(doc) { if (doc.Type == \""user\"") { emit(doc); } }""
                }
            }
        },
        {
            ""_id"": ""globalusagerights"",
            ""Type"": ""globalusagerights"",
            ""UsageRights"": [
                {
                    ""group-administrators"": 65535
                }
            ]
        },
        {
            ""_id"": ""group-administrators"",
            ""Type"": ""group"",
            ""Groups"": [
            ],
            ""Users"": [
                ""user-administrator""
            ]
        },
        {
            ""_id"": ""user-administrator"",
            ""Type"": ""user"",
            ""Password"": ""sQnzu7wkTrgkQZF+0G1hi5AI3Qmzvv0bXgc5THBqi7mAsdd4Xll27ASbRt9fEyavWi6m0QP9B8lThf+rDKy8hg=="",
            ""FirstName"": null,
            ""MiddleName"": null,
            ""LastName"": null,
            ""Superuser"": true,
            ""Groups"": [
                ""group-administrators""
            ]
        },
        {
            ""_id"": ""resourceusagerightstemplate"",
            ""Type"": ""resourceusagerightstemplate"",
            ""UsageRights"": [
                { 
                    ""group-administrators"": 31
                }
            ]
        }
    ]
}";

        public List<Commands.PostBulkDocumentsReply.Entry> Results;

        public Install(IDatabase db, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.UploadBulkDocuments(_db, new Model.BulkDocuments(_package), _sendTimeout, _receiveTimeout,
                    _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Tasks.UploadBulkDocuments task = (Tasks.UploadBulkDocuments)sender;
            Results = task.Results;
            TriggerOnComplete(reply, Results);
        }
    }
}
