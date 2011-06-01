using System;

namespace Common.Work.Tasks
{
    public class Checkout : Base
    {
        private ResourceJobBase _job;
        private FileSystem.IO _fileSystem;
        private int _sendBufferSize = 4096;
        private int _receiveBufferSize = 4096;

        public Checkout(ResourceJobBase job, FileSystem.IO fileSystem, int sendBufferSize, 
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
            Logger.General.Debug("Checkout command starting on job id " + _job.Id.ToString() + ".");

            if (_job.IsError || _job.CheckForAbortAndUpdate())
            {
                errorMessage = "The job's flag was set to error or abort.";
                return false;
            }
            
            resource.
            if (!resource.CheckoutResource(_job, _fileSystem, _sendBufferSize, _receiveBufferSize, out errorMessage))
            {
                Logger.General.Error("The checkout command failed in execution for job " + _job.Id.ToString() +
                    " with error message: " + errorMessage);
                return false;
            }

            _percentComplete = 100;
            ReportProgress();

            Logger.General.Debug("The checkout command succeeded in execution for job " + _job.Id.ToString() + ".");

            _job.UpdateLastAction();

            return true;
        }
    }
}
