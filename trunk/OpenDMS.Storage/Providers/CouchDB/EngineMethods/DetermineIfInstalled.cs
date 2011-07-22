using System;
using System.Collections.Generic;
using OpenDMS.Networking.Http;

namespace OpenDMS.Storage.Providers.CouchDB.EngineMethods
{
    public class DetermineIfInstalled : Base
    {
        private List<string> _idsToCheck;
        private int _idFinishedCount;
        private int _lastProgressFinishedCount;

        public DetermineIfInstalled(EngineRequest request)
            : base(request)
        {
            _idsToCheck = new List<string>();
            _idsToCheck.Add("_design/groups");
            _idsToCheck.Add("_design/users");
            _idsToCheck.Add("globalusagerights");
            _idsToCheck.Add("group-administrators");
            _idsToCheck.Add("user-administrator");
        }

        public override void Execute()
        {
            List<Commands.HeadDocument> cmds = new List<Commands.HeadDocument>();

            try
            {
                if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.Preparing, true);
            }
            catch (System.Exception e)
            {
                Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                throw;
            }

            for (int i = 0; i < _idsToCheck.Count; i++)
            {
                cmds.Add(new Commands.HeadDocument(UriAppend(_idsToCheck[i])));

                cmds[i].OnComplete += delegate(Commands.Base sender, Client client, Connection connection, Commands.ReplyBase reply)
                {
                    if (_isEventSubscriptionSuppressed) return;

                    Commands.HeadDocumentReply hReply = (Commands.HeadDocumentReply)reply;

                    if (hReply.IsError)
                    {
                        // If there is an error, block all other events and return the problem to the subscriber immediately
                        _isEventSubscriptionSuppressed = true;
                        _onComplete(_request, hReply);
                        return;
                    }

                    // Otherwise
                    _idFinishedCount++;
                                        
                    if (_idFinishedCount >= _idsToCheck.Count)
                    { // Done - return the most recent result
                        _onComplete(_request, hReply);
                    }
                };

                cmds[i].OnError += delegate(Commands.Base sender, Client client, string message, Exception exception)
                {
                    if (_isEventSubscriptionSuppressed) return;
                    _isEventSubscriptionSuppressed = true;
                    _onError(_request, message, exception);
                };

                cmds[i].OnProgress += delegate(Commands.Base sender, Client client, Connection connection, DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete)
                {
                    if (_isEventSubscriptionSuppressed) return;
                    if (_idFinishedCount > _lastProgressFinishedCount)
                    {
                        _lastProgressFinishedCount = _idFinishedCount;
                        decimal percentComplete = ((decimal)_lastProgressFinishedCount / (decimal)_idsToCheck.Count) * 100;
                        _onProgress(_request, direction, packetSize, percentComplete, percentComplete);
                    }
                };

                cmds[i].OnTimeout += delegate(Commands.Base sender, Client client, Connection connection)
                {
                    if (_isEventSubscriptionSuppressed) return;
                    _isEventSubscriptionSuppressed = true;
                    _onTimeout(_request);
                };

                _lastProgressFinishedCount = 0;

                try
                {
                    if (_onActionChanged != null) _onActionChanged(_request, EngineActionType.DeterminingInstallation, true);
                }
                catch (System.Exception e)
                {
                    Logger.Storage.Error("An exception occurred while calling the OnActionChanged event.", e);
                    throw;
                }

                cmds[i].Execute(_request.Database.Server.Timeout, _request.Database.Server.Timeout, _request.Database.Server.BufferSize, _request.Database.Server.BufferSize);
            }
        }

        protected override void GetResourcePermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Not called
            throw new NotImplementedException();
        }

        protected override void GetGlobalPermissions_OnComplete(EngineRequest request, ICommandReply reply)
        {
            // Never called
            throw new NotImplementedException();
        }

        private Uri UriAppend(string path)
        {
            return new Uri(_request.Database.Uri.ToString() + path);
        }
    }
}
