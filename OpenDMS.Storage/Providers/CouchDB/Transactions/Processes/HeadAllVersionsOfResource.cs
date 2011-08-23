using System;
using System.Collections.Generic;

namespace OpenDMS.Storage.Providers.CouchDB.Transactions.Processes
{
    public class HeadAllVersionsOfResource : Base
    {
        private Data.Resource _resource;
        private Security.RequestingPartyType _requestingPartyType;
        private Security.Session _session;
        private Dictionary<Data.VersionId, string> _revisions;
        private int _receivedCount;

        public HeadAllVersionsOfResource(IDatabase db, Data.Resource resource,
            Security.RequestingPartyType requestingPartyType, Security.Session session, int sendTimeout,
            int receiveTimeout, int sendBufferSize, int receiveBufferSize)
            : base(db, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize)
        {
            _resource = resource;
            _requestingPartyType = requestingPartyType;
            _session = session;
        }

        public override void Process()
        {
            RunTaskProcess(new Tasks.CheckResourcePermissions(_db, _resource, _requestingPartyType,
                _session, Security.Authorization.ResourcePermissionType.ReadOnly,
                _sendTimeout, _receiveTimeout, _sendBufferSize, _receiveBufferSize));
        }

        public override void TaskComplete(Tasks.Base sender, ICommandReply reply)
        {
            Type t = sender.GetType();
            
            if (t == typeof(Tasks.CheckResourcePermissions))
            {
                Tasks.CheckResourcePermissions task = (Tasks.CheckResourcePermissions)sender;
                if (!task.IsAuthorized)
                {
                    TriggerOnAuthorizationDenied(task);
                    return;
                }

                // First, we load up revisions
                for (int i = 0; i < _resource.VersionIds.Count; i++)
                {
                    _revisions.Add(_resource.VersionIds[i], null);
                }

                // Now our _revisions holds indexes for all version ids
                // Dispatch all our heads to get the revisions
                // *note* do not combine these loops - we want the full dictionary before starting
                for (int i = 0; i < _resource.VersionIds.Count; i++)
                {
                    RunTaskProcess(new Tasks.HeadVersion(_db, _resource.VersionIds[i], _sendTimeout, 
                        _receiveTimeout, _sendBufferSize, _receiveBufferSize));
                }
            }
            else if (t == typeof(Tasks.HeadVersion))
            {
                Tasks.HeadVersion task = (Tasks.HeadVersion)sender;
                
                if (!_revisions.ContainsKey(task.VersionId))
                {
                    TriggerOnError(task, "The id '" + task.VersionId.ToString() + "' could not be found.", new KeyNotFoundException());
                    return;
                }
                
                lock (_revisions)
                {
                    _receivedCount++;
                    _revisions[task.VersionId] = task.Revision;
                    if (_revisions.Count == _receivedCount)
                    {
                        TriggerOnComplete(reply, _revisions);
                    }
                    else
                    {
                        TriggerOnProgress(task, Networking.Http.DirectionType.Download, -1, -1,
                            ((decimal)((decimal)_receivedCount / (decimal)_revisions.Count)));
                    }
                }
            }
            else
            {
                TriggerOnError(sender, reply.ToString(), null);
            }
        }
    }
}
