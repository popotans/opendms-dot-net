using System;
using System.Text;

namespace Common.Work.Tasks
{
    public class CreateResource : Base
    {
        private ResourceJobBase _job;
        private FileSystem.IO _fileSystem;
        private int _sendBufferSize = 4096;
        private int _receiveBufferSize = 4096;

        public CreateResource(ResourceJobBase job, FileSystem.IO fileSystem, int sendBufferSize, 
            int receiveBufferSize)
        {
            _job = job;
            _fileSystem = fileSystem;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        public override bool Execute(Storage.Version resource, out string errorMessage)
        {
            FileSystem.MetaResource mr;
            FileSystem.DataResource dr;
            Common.Postgres.Version pgVersion;

            errorMessage = null;
            Logger.General.Debug("CreateResource command starting on job id " + _job.Id.ToString() + ".");

            if (_job.IsError || _job.CheckForAbortAndUpdate())
            {
                errorMessage = "The job's flag was set to error or abort.";
                return false;
            }

            // Postgres work
            Postgres.Resource.CreateNewResource(_job.RequestingUser, out pgVersion);

            // Rename files to the proper new GUID
            mr = new FileSystem.MetaResource(resource.MetaAsset, _fileSystem);
            dr = new FileSystem.DataResource(resource.DataAsset, _fileSystem);

            mr.DeleteFromFilesystem();
            dr.Rename(pgVersion.VersionGuid);

            // Assign the GUID received from Postgres to our internal objects
            Logger.General.Debug("Translating guid of " + resource.MetaAsset.Guid.ToString("N") + " to " + pgVersion.VersionGuid.ToString("N"));
            _job.Requestor.ServerTranslation(_job, resource.MetaAsset.Guid, pgVersion.VersionGuid);
            resource.MetaAsset.Guid = resource.DataAsset.Guid = pgVersion.VersionGuid;

            // Save MA
            resource.MetaAsset.SaveToLocal(_job, _fileSystem);
            
            resource.DataAsset.OnUploadProgress += new Storage.DataAsset.ProgressHandler(DataAsset_OnUploadProgress);
            resource.DataAsset.OnTimeout += new Storage.DataAsset.EventHandler(DataAsset_OnTimeout);

            if (!resource.CreateResourceOnRemote(_job, _fileSystem, _sendBufferSize, _receiveBufferSize, out errorMessage))
            {
                Logger.General.Error("The CreateResource command failed in execution for job " + _job.Id.ToString() +
                    " with error message: " + errorMessage);
                return false;
            }

            // No need to monitor this event anymore
            resource.DataAsset.OnUploadProgress -= DataAsset_OnUploadProgress;
            resource.DataAsset.OnTimeout -= DataAsset_OnTimeout;

            Logger.General.Debug("The CreateResource command succeeded in execution for job " + _job.Id.ToString() + ".");

            _job.UpdateLastAction();

            return true;
        }

        void DataAsset_OnTimeout(Storage.DataAsset sender)
        {
            _job.ActivateTimeoutFlag();
        }

        void DataAsset_OnUploadProgress(Storage.DataAsset sender, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            _job.UpdateProgress((ulong)sender.BytesComplete, (ulong)sender.BytesTotal);
            Logger.General.Debug("CreateResourceJob with id " + _job.Id.ToString() + " is now " + _job.PercentComplete.ToString() + "% complete.");

            // Don't update the UI if finished, the final update is handled by the Run() method.
            if (sender.BytesComplete != sender.BytesTotal)
                _job.ReportWork(_job);
        }
    }
}
