using System;

namespace Common.Work.Tasks
{
    public class GetData : Base
    {
        private ResourceJobBase _job;
        private FileSystem.IO _fileSystem;
        private int _sendBufferSize = 4096;
        private int _receiveBufferSize = 4096;

        public GetData(ResourceJobBase job, FileSystem.IO fileSystem, int sendBufferSize,
            int receiveBufferSize)
        {
            _job = job;
            _fileSystem = fileSystem;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        public override bool Execute(Storage.Version resource, out string errorMessage)
        {
            errorMessage = null;
            Logger.General.Debug("GetData command starting on job id " + _job.Id.ToString() + ".");

            if (_job.IsError || _job.CheckForAbortAndUpdate())
            {
                errorMessage = "The job's flag was set to error or abort.";
                return false;
            }

            resource.DataAsset.OnDownloadProgress += new Storage.DataAsset.ProgressHandler(DataAsset_OnDownloadProgress);
            resource.DataAsset.OnTimeout += new Storage.DataAsset.EventHandler(DataAsset_OnTimeout);

            if (!resource.DownloadDataAssetAndSaveLocally(_job, _fileSystem, _sendBufferSize, _receiveBufferSize, out errorMessage))
            {
                Logger.General.Error("The GetData command failed in execution for job " + _job.Id.ToString() +
                    " with error message: " + errorMessage);
                return false;
            }

            Logger.General.Debug("The GetData command succeeded in execution for job " + _job.Id.ToString() + ".");

            _job.UpdateLastAction();

            return true;
        }

        void DataAsset_OnTimeout(Storage.DataAsset sender)
        {
            _job.ActivateTimeoutFlag();
        }

        void DataAsset_OnDownloadProgress(Storage.DataAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            _job.UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            Logger.General.Debug("The GetData command runnig on job id " + _job.Id.ToString() + " is now " + _job.PercentComplete.ToString() + "% complete.");

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _job.ReportWork(_job);
        }
    }
}
