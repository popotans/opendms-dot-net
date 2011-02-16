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
    /// <summary>
    /// Represents an object that manages jobs.
    /// </summary>
    public class Master
    {
        /// <summary>
        /// An enumeration of types of jobs
        /// </summary>
        public enum JobType
        {
            /// <summary>
            /// No job type set.
            /// </summary>
            None = 0,
            /// <summary>
            /// Downloads the ETag from the remote host.
            /// </summary>
            GetETag,
            /// <summary>
            /// Downloads the full asset from the remote host.
            /// </summary>
            DownloadAsset,
            /// <summary>
            /// If the remote ETag is newer then downloads the full asset from the 
            /// remote host, saving it to disk and updating the local meta asset.
            /// </summary>
            LoadResource,
            /// <summary>
            /// Uploads the full asset to the remote host.
            /// </summary>
            SaveResource,
            /// <summary>
            /// Uploads the full asset to the remote host creating a new resource.
            /// </summary>
            CreateResource,
            /// <summary>
            /// Downloads the Header information (ETag and MD5) from the remote host.
            /// </summary>
            GetHead,
            /// <summary>
            /// Updates the MetaAsset on the server applying a lock
            /// </summary>
            Lock,
            /// <summary>
            /// Updates the MetaAsset on the server releasing the lock
            /// </summary>
            Unlock
        }

        /// <summary>
        /// A collection of jobs waiting for execution
        /// </summary>
        private List<AssetJobBase> _jobQueue;
        /// <summary>
        /// A collection of jobs currently executing
        /// </summary>
        private List<AssetJobBase> _executingJobs;
        /// <summary>
        /// A collection of <see cref="Guid"/> object representing the IDs of locked resources.
        /// </summary>
        private List<Guid> _lockedResourceIDs;
        /// <summary>
        /// The <see cref="Thread"/> responsible for dispatching jobs.
        /// </summary>
        private Thread _workDispatcher;
        /// <summary>
        /// The id of the next job.
        /// </summary>
        private ulong _id;
        /// <summary>
        /// A reference to the <see cref="ErrorManager"/>.
        /// </summary>
        private ErrorManager _errorManager;
        /// <summary>
        /// A reference to the <see cref="FileSystem.IO"/>.
        /// </summary>
        private FileSystem.IO _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Master"/> class.
        /// </summary>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public Master(ErrorManager errorManager, FileSystem.IO fileSystem)
        {
            _jobQueue = new List<AssetJobBase>();
            _executingJobs = new List<AssetJobBase>();
            _lockedResourceIDs = new List<Guid>();
            _workDispatcher = null;
            _id = 1;
            _errorManager = errorManager;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Adds a work request to a list of pending requests.  The <see cref="Master"/> will assign
        /// the request to a job and issue it to a worker thread as soon as it determines it is possible.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="jobType">The <see cref="JobType"/> of the job.</param>
        /// <param name="fullAsset">The full asset.</param>
        /// <param name="actUpdateUI">The method called to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
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
                            actUpdateUI, timeout, _errorManager, _fileSystem);
                        break;
                    case JobType.GetHead:
                        job = new GetHeadJob(requestor, _id++, fullAsset,
                            actUpdateUI, timeout, _errorManager, _fileSystem);
                        break;
                    case JobType.DownloadAsset:
                        job = new DownloadAssetJob(requestor, _id++, fullAsset, actUpdateUI, timeout, 
                            _errorManager, _fileSystem);
                        break;
                    case JobType.LoadResource:
                        job = new LoadResourceJob(requestor, _id++, fullAsset, actUpdateUI, timeout, _errorManager, 
                            _fileSystem);
                        break;
                    case JobType.SaveResource:
                        job = new SaveResourceJob(requestor, _id++, fullAsset, actUpdateUI, timeout,
                            _errorManager, _fileSystem);
                        break;
                    case JobType.CreateResource:
                        job = new CreateResourceJob(requestor, _id++, fullAsset, actUpdateUI, timeout,
                            _errorManager, _fileSystem);
                        break;
                    case JobType.Lock:
                        job = new LockJob(requestor, _id++, fullAsset,
                            actUpdateUI, timeout, _errorManager, _fileSystem);
                        break;
                    case JobType.Unlock:
                        job = new UnlockJob(requestor, _id++, fullAsset,
                            actUpdateUI, timeout, _errorManager, _fileSystem);
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
                        if (!AssetIsLocked(_jobQueue[pos].FullAsset))
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

        /// <summary>
        /// Checks if the specified asset is locked
        /// </summary>
        /// <param name="fullAsset">The full asset.</param>
        /// <returns><c>True</c> if locked; otherwise, <c>false</c>.</returns>
        private bool AssetIsLocked(Data.FullAsset fullAsset)
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

        /// <summary>
        /// Starts a worker thread to execute a job.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>The thread running the job.</returns>
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

        /// <summary>
        /// Runs the job.
        /// </summary>
        /// <param name="obj">The job.</param>
        private void RunJob(object obj)
        {
            if (obj.GetType().BaseType != typeof(AssetJobBase))
                throw new ArgumentException("Argument must be of type Work.AssetJobBase");

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

        /// <summary>
        /// Cancels the currently executing job for a resource.
        /// </summary>
        /// <param name="fullAsset">The full asset.</param>
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

        /// <summary>
        /// Finds the executing job for resource.
        /// </summary>
        /// <param name="fullAsset">The full asset.</param>
        /// <returns>The <see cref="AssetJobBase"/> if found; otherwise, <c>null</c>.</returns>
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
