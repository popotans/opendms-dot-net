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

namespace Common.Work
{
    /// <summary>
    /// An abstract class that represents the base requirements for any inheriting class
    /// </summary>
    public abstract class ResourceJobBase : JobBase
    {
        protected JobArgs _args;
        protected CouchDB.Database _couchdb;
        protected Storage.Resource _jobResource;

        /// <summary>
        /// A reference to a <see cref="Storage.Resource"/> which was passed to this job.
        /// </summary>
        protected Storage.Resource _inputResource;
        /// <summary>
        /// Gets a reference to the <see cref="Storage.Resource"/> which was passed to this job.
        /// </summary>
        public Storage.Resource InputResource { get { return _inputResource; } }
        /// <summary>
        /// Gets a reference to the <see cref="Storage.Resource"/> which was produced by this job.
        /// </summary>
        public Storage.Resource ResultResource { get { return _jobResource; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceJobBase"/> class.
        /// </summary>
        /// <param name="args">The <see cref="JobArgs"/>.</param>
        public ResourceJobBase(JobArgs args)
            : base(args)
        {
            _args = args;
            _inputResource = args.Resource;
            _couchdb = args.CouchDB;
            if (this.GetType() == typeof(GetResourceJob) ||
                this.GetType() == typeof(CheckoutJob))
            {
                // Here we just create a new resource, because we do not care what exists locally
                // we will be overwriting it anyway.
                _jobResource = new Storage.Resource(_inputResource.Guid, _couchdb);
            }
            else
                _jobResource = Storage.Resource.DeepCopy(_inputResource);
        }

        protected void ReportWork(ResourceJobBase job)
        {
            _requestor.WorkReport(new JobResult() { Resource = _jobResource, 
                                                    Job = job, 
                                                    InputArgs = _args });
        }
    }
}
