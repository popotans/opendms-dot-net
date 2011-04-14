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
        protected Storage.Resource _jobResource;

        /// <summary>
        /// A reference to a <see cref="Storage.Resource"/> which was passed to this job.
        /// </summary>
        protected Storage.Resource _inputResource;
        /// <summary>
        /// Gets a reference to the <see cref="Storage.Resource"/> which was passed to this job.
        /// </summary>
        public Storage.Resource Resource { get { return _inputResource; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceJobBase"/> class.
        /// </summary>
        /// <param name="requestor">The object that requested performance of this job.</param>
        /// <param name="id">The id of this job.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="actUpdateUI">The method to call to update the UI.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="progressMethod">The <see cref="T:ProgressMethodType"/>.</param>
        /// <param name="errorManager">A reference to the <see cref="ErrorManager"/>.</param>
        /// <param name="fileSystem">A reference to the <see cref="FileSystem.IO"/>.</param>
        public ResourceJobBase(IWorkRequestor requestor, ulong id, Storage.Resource resource, 
            UpdateUIDelegate actUpdateUI, uint timeout, ProgressMethodType progressMethod, 
            ErrorManager errorManager)
            : base(requestor, id, actUpdateUI, timeout, progressMethod, errorManager)
        {
            _inputResource = resource;
            _jobResource = Storage.Resource.DeepCopy(resource);
        }
    }
}
