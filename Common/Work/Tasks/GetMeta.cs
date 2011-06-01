using System;

namespace Common.Work.Tasks
{
    public class GetMeta : Base
    {
        private ResourceJobBase _job;
        private int _sendBufferSize = 4096;
        private int _receiveBufferSize = 4096;

        public GetMeta(ResourceJobBase job, int sendBufferSize, int receiveBufferSize)
        {
            _job = job;
            _sendBufferSize = sendBufferSize;
            _receiveBufferSize = receiveBufferSize;
        }

        public override bool Execute(Storage.Version resource, out string errorMessage)
        {
            errorMessage = null;
            Logger.General.Debug("GetMetaOnly command starting on job id " + _job.Id.ToString() + ".");

            if (_job.IsError || _job.CheckForAbortAndUpdate())
            {
                errorMessage = "The job's flag was set to error or abort.";
                return false;
            }

            if (!resource.GetMetaAssetFromRemote(_job, _sendBufferSize, _receiveBufferSize, out errorMessage))
            {
                Logger.General.Error("The GetMetaOnly command failed in execution for job " + _job.Id.ToString() +
                    " with error message: " + errorMessage);
                return false;
            }

            Logger.General.Debug("The GetMetaOnly command succeeded in execution for job " + _job.Id.ToString() + ".");

            _job.UpdateLastAction();

            return true;
        }
    }
}
