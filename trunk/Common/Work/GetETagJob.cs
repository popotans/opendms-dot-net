using System;

namespace Common.Work
{
    public class GetETagJob : AssetJobBase
    {
        public Data.ETag ETag;

        public GetETagJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager, 
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, 
            ProgressMethodType.Indeterminate, errorManager, fileSystem, generalLogger, 
            networkLogger)
        {
            ETag = null;
        }

        public override JobBase Run()
        {
            _currentState = State.Active | State.Executing;

            try
            {
                StartTimeout();
            }
            catch(Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "GetETagJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (_fullAsset.MetaAsset == null)
            {
                _errorManager.AddError(ErrorMessage.GetETagFailedDueToInvalidState(this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            try
            {
                ETag = _fullAsset.GetETagFromServer(this, _networkLogger);
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.GetETagFailed(e, this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (this.IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }
    }
}
