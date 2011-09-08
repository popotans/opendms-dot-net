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
            ""_id"": ""_design/search"",
            ""language"": ""javascript"",
            ""fulltext"": {
                ""main"": {
                    ""index"": ""function(doc) { if (doc.$type == \""resource\"") { var ret = new Document(); ret.add(doc._id, {\""field\"":\""id\""}); ret.add(doc.$type, {\""field\"":\""type\"", \""store\"":\""yes\""}); ret.add(doc.$created, {\""field\"":\""created\"", \""store\"":\""yes\""}); ret.add(doc.$creator, {\""field\"":\""creator\"", \""store\"":\""yes\""}); ret.add(doc.$modified, {\""field\"":\""modified\"", \""store\"":\""yes\""}); ret.add(doc.$modifier, {\""field\"":\""modifier\"", \""store\"":\""yes\""}); ret.add(doc.$checkedoutat, {\""field\"":\""checkedoutat\"", \""store\"":\""yes\""}); ret.add(doc.$checkedoutto, {\""field\"":\""checkedoutto\"", \""store\"":\""yes\""}); ret.add(doc.$lastcommit, {\""field\"":\""lastcommit\"", \""store\"":\""yes\""}); ret.add(doc.$lastcommitter, {\""field\"":\""lastcommitter\"", \""store\"":\""yes\""}); ret.add(doc.$title, {\""field\"":\""title\"", \""store\"":\""yes\""}); var arr = []; for(var i in doc.$tags) { arr.push(doc.$tags[i]); } ret.add(arr.join(', '), {\""field\"":\""tags\"", \""store\"":\""yes\""}); arr = []; for(var i in doc.$usagerights) { for(var j in doc.$usagerights[i]) { arr.push(j + \"":\"" + doc.$usagerights[i][j]); } } ret.add(arr.join(', '), {\""field\"":\""usagerights\"", \""store\"":\""yes\""});return ret; } else if (doc.$type == \""version\"") { var ret = new Document(); ret.add(doc._id, {\""field\"":\""id\""}); ret.add(doc.$type, {\""field\"":\""type\"", \""store\"":\""yes\""}); ret.add(doc.$md5, {\""field\"":\""md5\"", \""store\"":\""yes\""}); ret.add(doc.$extension, {\""field\"":\""extension\"", \""store\"":\""yes\""}); ret.add(doc.$created, {\""field\"":\""created\"", \""store\"":\""yes\""}); ret.add(doc.$creator, {\""field\"":\""creator\"", \""store\"":\""yes\""}); ret.add(doc.$modified, {\""field\"":\""modified\"", \""store\"":\""yes\""}); ret.add(doc.$modifier, {\""field\"":\""modifier\"", \""store\"":\""yes\""}); for(var i in doc._attachments) { ret.attachment(\""attachment\"", i); } return ret; } }""
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
