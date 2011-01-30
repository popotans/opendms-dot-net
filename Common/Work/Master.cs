/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;
using System.Collections.Generic;

namespace Common.Work
{
    public class Master
    {
        public enum JobType
        {
            None = 0,
            GetETag,
            DownloadAsset,
            LoadResource,
            SaveResource
        }

        private List<AssetJobBase> _jobQueue;
        private List<AssetJobBase> _executingJobs;
        private List<Guid> _lockedResourceIDs;
        private Thread _workDispatcher;
        private ulong _id;
        private ErrorManager _errorManager;
        private FileSystem.IO _fileSystem;
        private Logger _generalLogger;
        private Logger _networkLogger;

        public Master(ErrorManager errorManager, FileSystem.IO fileSystem, 
            Logger generalLogger, Logger networkLogger)
        {
            _jobQueue = new List<AssetJobBase>();
            _executingJobs = new List<AssetJobBase>();
            _lockedResourceIDs = new List<Guid>();
            _workDispatcher = null;
            _id = 1;
            _errorManager = errorManager;
            _fileSystem = fileSystem;
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;
        }

        public void AddJob(IWorkRequestor requestor, JobType jobType, Data.FullAsset fullAsset, 
            AssetJobBase.UpdateUIDelegate actUpdateUI, uint timeout)
        {
            AssetJobBase job = null;

            try
            {
                switch (jobType)
                {
                    case JobType.GetETag:
                        job = new GetETagJob(requestor, _id++, fullAsset,
                            actUpdateUI, timeout, _errorManager, _fileSystem, _generalLogger, 
                            _networkLogger);
                        break;
                    case JobType.DownloadAsset:
                        job = new DownloadAssetJob(requestor, _id++, fullAsset, actUpdateUI, timeout, 
                            _errorManager, _fileSystem, _generalLogger, _networkLogger);
                        break;
                    case JobType.LoadResource:
                        job = new LoadResourceJob(requestor, _id++, fullAsset, actUpdateUI, timeout, _errorManager, 
                            _fileSystem, _generalLogger, _networkLogger);
                        break;
                    case JobType.SaveResource:
                        job = new SaveResourceJob(requestor, _id++, fullAsset, actUpdateUI, timeout, 
                            _errorManager, _fileSystem, _generalLogger, _networkLogger);
                        break;
                    default:
                        throw new Exception("Unknown job type");
                }
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.JobCreateFailed(e, fullAsset, _id));
                return;
            }

            try
            {
                lock (_jobQueue)
                {
                    _jobQueue.Add(job);
                    if (_workDispatcher == null)
                    {
                        _workDispatcher = new Thread(PollForWork);
                        _workDispatcher.Start();
                    }
                }            
            }
            catch (Exception e)
            {
                lock (_jobQueue)
                {
                    if (_jobQueue.Contains(job))
                    {
                        _jobQueue.Remove(job);
                    }
                }
                _errorManager.AddError(ErrorMessage.JobStartFailed(e, job));
            }
        }

        /// <summary>
        /// Dispatches any work in the job queue then kills itself
        /// </summary>
        private void PollForWork()
        {
            int pos = 0;

            try
            {
                while (_jobQueue.Count > 0)
                {
                    pos = 0; // Start at 0
                    while (pos < _jobQueue.Count)
                    {
                        // Run through the queue looking for jobs that can be performed
                        if (!ResourceIsLocked(_jobQueue[pos].FullAsset))
                        {
                            lock (_jobQueue)
                            {
                                try { StartJob(_jobQueue[pos]); }
                                catch (Exception e)
                                {
                                    _errorManager.AddError(ErrorMessage.JobStartFailed(e, _jobQueue[pos]));
                                }
                                _jobQueue.Remove(_jobQueue[pos]);
                            }
                        }
                        else
                            pos++;
                    }
                    Thread.Sleep(100); // Sleep for 1/10th of a second
                }
            }
            catch (Exception e)
            {
                _errorManager.AddError(new ErrorMessage(0, "Error While Working", 
                    "An error occurred while processing operations.  This operation has terminated, you might need to retry.", 
                    "An unknown error occurred while polling for new work.", true, true, e));
            }

            lock (_workDispatcher) { _workDispatcher = null; }
        }

        private bool ResourceIsLocked(Data.FullAsset fullAsset)
        {
            lock (_lockedResourceIDs)
            {
                for (int i = 0; i < _lockedResourceIDs.Count; i++)
                {
                    if (_lockedResourceIDs[i] == fullAsset.Guid)
                        return true;
                }
            }
            
            return false;
        }

        public Thread StartJob(AssetJobBase job)
        {
            Thread t = new Thread(RunJob);

            lock (_executingJobs)
            {
                lock (_lockedResourceIDs)
                {
                    _lockedResourceIDs.Add(job.FullAsset.Guid);
                }
                _executingJobs.Add(job);
            }

            try
            {
                t.Start(job);
            }
            catch (Exception e)
            {
                lock (_executingJobs)
                {
                    lock (_lockedResourceIDs)
                    {
                        if (_lockedResourceIDs.Contains(job.FullAsset.Guid))
                            _lockedResourceIDs.Remove(job.FullAsset.Guid);
                    }
                    if(_executingJobs.Contains(job)) _executingJobs.Remove(job);
                }

                _errorManager.AddError(ErrorMessage.JobStartFailed(e, job));
            }

            return t;
        }

        private void RunJob(object obj)
        {
            if (obj.GetType().BaseType != typeof(AssetJobBase))
                throw new ArgumentException("Argument must be of type Work.Job");

            AssetJobBase job = (AssetJobBase)obj;
            lock (job)
            {
                lock (job.FullAsset)
                {
                    try
                    {
                        job.Run();
                    }
                    catch (Exception e)
                    {
                        lock (_executingJobs)
                        {
                            lock (_lockedResourceIDs)
                            {
                                if (_lockedResourceIDs.Contains(job.FullAsset.Guid))
                                    _lockedResourceIDs.Remove(job.FullAsset.Guid);
                            }
                            if (_executingJobs.Contains(job)) _executingJobs.Remove(job);
                        }

                        _errorManager.AddError(ErrorMessage.JobRunFailed(e, job));
                    }
                }
                
                lock (_executingJobs)
                {
                    lock (_lockedResourceIDs)
                    {
                        _lockedResourceIDs.Remove(job.FullAsset.Guid);
                    }
                    _executingJobs.Remove(job);
                }
            }
        }

        public void CancelJobForResource(Data.FullAsset fullAsset)
        {
            lock (_executingJobs)
            {
                for (int i = 0; i < _executingJobs.Count; i++)
                {
                    if (_executingJobs[i].FullAsset == fullAsset)
                    {
                        _executingJobs[i].Cancel();
                    }
                }
            }
        }

        public AssetJobBase FindExecutingJobForResource(Data.FullAsset fullAsset)
        {
            lock (_executingJobs)
            {
                for (int i = 0; i < _executingJobs.Count; i++)
                {
                    if (_executingJobs[i].FullAsset == fullAsset)
                    {
                        return _executingJobs[i];
                    }
                }
            }

            return null;
        }
    }
}
