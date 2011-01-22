using System;

namespace Common.Work
{
    public class SaveResourceJob 
        : AssetJobBase
    {
        public SaveResourceJob(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset,
            UpdateUIDelegate actUpdateUI, uint timeout, ErrorManager errorManager,
            FileSystem.IO fileSystem, Logger generalLogger, Logger networkLogger)
            : base(requestor, id, fullAsset, actUpdateUI, timeout, ProgressMethodType.Determinate,
            errorManager, fileSystem, generalLogger, networkLogger)
        {
        }

        public override JobBase Run()
        {
            _currentState = State.Active | State.Executing;

            try
            {
                StartTimeout();
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.TimeoutFailedToStart(e, this, "SaveResourceJob"));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            _fullAsset.DataAsset.OnProgress += new Data.DataAsset.ProgressHandler(Run_DataAsset_OnProgress);

            if (!_fullAsset.SaveToServer(this, _networkLogger))
            {
                _errorManager.AddError(ErrorMessage.SaveResourceFailed(null, this, 
                    "Please review the log file for details", "Review prior log entries for details."));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            // No need to monitor this event anymore
            _fullAsset.DataAsset.OnProgress -= Run_DataAsset_OnProgress;

            if (IsError || CheckForAbortAndUpdate())
            {
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            // Now we need to update the local meta asset - version #s, etags and such
            try
            {
                // Downloads it
                _fullAsset.MetaAsset.DownloadFromServer(this, _networkLogger);
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.LoadResourceFailed(e, this));
                _currentState = State.Error;
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
                return this;
            }

            // Below was commented out because the DownloadFromServer() above has a save built-in.

            //// Save it locally
            //try
            //{
            //    _fullAsset.MetaAsset.Save();
            //}
            //catch (Exception e)
            //{
            //    if (_generalLogger != null)
            //        _generalLogger.Write(Logger.LevelEnum.Normal, "An exception occurred while " +
            //            "attempting to save the meta asset to the file system.\r\n" + 
            //            Logger.ExceptionToString(e));
            //    throw e;
            //}

            // Below is removed because the metaasset is loaded into memory when it is downloaded
            // from the server, thus, this is an unnecessary process
            // Reload the local resource
            //_resource.MetaAsset.LoadFromLocal(_fileSystem, _generalLogger);
            

            _currentState = State.Active | State.Finished;
            _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
            return this;
        }

        void Run_DataAsset_OnProgress(Data.DataAsset sender, int percentComplete)
        {
            UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _requestor.WorkReport(_actUpdateUI, this, _fullAsset);
        }
    }
}
