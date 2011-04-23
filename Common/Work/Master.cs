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
            /// Downloads the resource from the remote host.
            /// </summary>
            GetResource,
            /// <summary>
            /// Uploads the resource to the remote host.
            /// </summary>
            SaveResource,
            /// <summary>
            /// Uploads the resource to the remote host creating a new resource.
            /// </summary>
            CreateResource,
            /// <summary>
            /// Applies a lock
            /// </summary>
            Lock,
            /// <summary>
            /// Releases the lock
            /// </summary>
            Unlock,
            /// <summary>
            /// Checks whether the client and server files match
            /// </summary>
            CheckUpdateStatus
        }

        /// <summary>
        /// A collection of jobs waiting for execution
        /// </summary>
        private List<ResourceJobBase> _jobQueue;
        /// <summary>
        /// A collection of jobs currently executing
        /// </summary>
        private List<ResourceJobBase> _executingJobs;
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
        /// A reference to the <see cref="CouchDB.Database"/>.
        /// </summary>
        private CouchDB.Database _couchdb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Master"/> class.
        /// </summary>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        /// <param name="couchdb">A reference to the <see cref="CouchDB.Database"/>.</param>
        public Master(ErrorManager errorManager, FileSystem.IO fileSystem, CouchDB.Database couchdb)
        {
            _jobQueue = new List<ResourceJobBase>();
            _executingJobs = new List<ResourceJobBase>();
            _lockedResourceIDs = new List<Guid>();
            _workDispatcher = null;
            _id = 1;
            _errorManager = errorManager;
            _fileSystem = fileSystem;
            _couchdb = couchdb;
        }

        /// <summary>
        /// Adds the job.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/></param>
        /// <param name="jobType">Type of the job.</param>
        public void AddJob(JobArgs args)
        {
            ResourceJobBase job = null;

            if (args.ErrorManager == null) args.ErrorManager = _errorManager;
            if (args.FileSystem == null) args.FileSystem = _fileSystem;
            if (args.CouchDB == null) args.CouchDB = _couchdb;

            args.Id = _id;
            _id++;

            try
            {
                switch (args.JobType)
                {
                    case JobType.GetResource:
                        job = new GetResourceJob(args);
                        break;
                    case JobType.SaveResource:
                        job = new SaveResourceJob(args);
                        break;
                    case JobType.CreateResource:
                        job = new CreateResourceJob(args);
                        break;
                    case JobType.Lock:
                        job = new LockJob(args);
                        break;
                    case JobType.Unlock:
                        job = new UnlockJob(args);
                        break;
                    case JobType.CheckUpdateStatus:
                        job = new CheckUpdateStatusJob(args);
                        break;
                    default:
                        throw new Exception("Unknown job type");
                }
            }
            catch (Exception e)
            {
                _errorManager.AddError(ErrorMessage.ErrorCode.JobCreationFailed,
                    "Failed to Create Job",
                    "You just took an action for which I am to create a job to accomplish that action, but I was unable to complete that request for an unknown reason, please check the logs.",
                    "Common.Work.Master.AddJob() threw an exception while attempting to instantiate a new job with id " + _id.ToString() +".",
                    true, true, e);
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
                _errorManager.AddError(ErrorMessage.ErrorCode.JobStartFailed,
                    "Failed to Start Job",
                    "You just took an action for which I am to start a job to accomplish that action, but I was unable to complete that request for an unknown reason, please check the logs.",
                    "Common.Work.Master.AddJob() threw an exception while attempting to start a new job with id " + _id.ToString() + ".",
                    true, true, e);
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
                        if (!AssetIsLocked(_jobQueue[pos].Resource))
                        {
                            lock (_jobQueue)
                            {
                                try { StartJob(_jobQueue[pos]); }
                                catch (Exception e)
                                {
                                    _errorManager.AddError(ErrorMessage.ErrorCode.JobStartFailed,
                                        "Failed to Start Job",
                                        "You just took an action for which I am to start a job to accomplish that action, but I was unable to complete that request for an unknown reason, please check the logs.",
                                        "Common.Work.Master.PollForWork() threw an exception while attempting to start a new job with id " + _jobQueue[pos].Id.ToString() + ".",
                                        true, true, e);
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
                _errorManager.AddError(ErrorMessage.ErrorCode.JobWorkingFailed,
                    "Error While Working",
                    "An error occurred while processing the requested actions.  This operation has terminated, you might need to retry.",
                    "Common.Work.Master.PollForWork() threw an exception while polling for new work on job id " + _jobQueue[pos].Id.ToString() + ".",
                    true, true, e);
            }

            lock (_workDispatcher) { _workDispatcher = null; }
        }

        /// <summary>
        /// Checks if the specified asset is locked
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns><c>True</c> if locked; otherwise, <c>false</c>.</returns>
        private bool AssetIsLocked(Storage.Resource resource)
        {
            lock (_lockedResourceIDs)
            {
                for (int i = 0; i < _lockedResourceIDs.Count; i++)
                {
                    if (_lockedResourceIDs[i] == resource.Guid)
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
        public Thread StartJob(ResourceJobBase job)
        {
            Thread t = new Thread(RunJob);

            lock (_executingJobs)
            {
                lock (_lockedResourceIDs)
                {
                    _lockedResourceIDs.Add(job.Resource.Guid);
                }
                _executingJobs.Add(job);
            }

            Logger.General.Debug("Job with id " + job.Id.ToString() + " is starting.");

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
                        if (_lockedResourceIDs.Contains(job.Resource.Guid))
                            _lockedResourceIDs.Remove(job.Resource.Guid);
                    }
                    if(_executingJobs.Contains(job)) _executingJobs.Remove(job);
                }

                _errorManager.AddError(ErrorMessage.ErrorCode.JobStartFailed,
                    "Failed to Start Job",
                    "You just took an action for which I am to start a job to accomplish that action, but I was unable to complete that request for an unknown reason, please check the logs.",
                    "Common.Work.Master.StartJob() threw an exception while attempting to start a new job with id " + job.Id.ToString() + ".",
                    true, true, e);
            }

            Logger.General.Debug("Job with id " + job.Id.ToString() + " has started.");

            return t;
        }

        /// <summary>
        /// Runs the job.
        /// </summary>
        /// <param name="obj">The job.</param>
        private void RunJob(object obj)
        {
            if (obj.GetType().BaseType != typeof(ResourceJobBase))
                throw new ArgumentException("Argument must be of type Work.AssetJobBase");

            ResourceJobBase job = (ResourceJobBase)obj;
            lock (job)
            {
                lock (job.Resource)
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
                                if (_lockedResourceIDs.Contains(job.Resource.Guid))
                                    _lockedResourceIDs.Remove(job.Resource.Guid);
                            }
                            if (_executingJobs.Contains(job)) _executingJobs.Remove(job);
                        }

                        _errorManager.AddError(ErrorMessage.ErrorCode.JobRunFailed,
                            "Failed to Run Job",
                            "You just took an action for which I am to run a job to accomplish that action, but I was unable to complete that request for an unknown reason, please check the logs.",
                            "Common.Work.Master.RunJob() threw an exception while attempting to run the job with id " + job.Id.ToString() + ".",
                            true, true, e);
                    }
                }
                
                lock (_executingJobs)
                {
                    lock (_lockedResourceIDs)
                    {
                        _lockedResourceIDs.Remove(job.Resource.Guid);
                    }
                    _executingJobs.Remove(job);
                }

                Logger.General.Debug("Job with id " + job.Id.ToString() + " has completed.");
            }
        }

        /// <summary>
        /// Cancels the currently executing job for a resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void CancelJobForResource(Storage.Resource resource)
        {
            lock (_executingJobs)
            {
                for (int i = 0; i < _executingJobs.Count; i++)
                {
                    if (_executingJobs[i].Resource == resource)
                    {
                        _executingJobs[i].Cancel();
                        Logger.General.Debug("Job with id " + _executingJobs[i].Id.ToString() + " has been canceled.");
                    }
                }
            }
        }

        /// <summary>
        /// Finds the executing job for resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The <see cref="ResourceJobBase"/> if found; otherwise, <c>null</c>.</returns>
        public ResourceJobBase FindExecutingJobForResource(Storage.Resource resource)
        {
            lock (_executingJobs)
            {
                for (int i = 0; i < _executingJobs.Count; i++)
                {
                    if (_executingJobs[i].Resource == resource)
                    {
                        return _executingJobs[i];
                    }
                }
            }

            return null;
        }
    }
}
