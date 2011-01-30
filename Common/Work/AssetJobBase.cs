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
    public abstract class AssetJobBase : JobBase
    {
        protected Data.FullAsset _fullAsset;
        public Data.FullAsset FullAsset { get { return _fullAsset; } }

        public AssetJobBase(IWorkRequestor requestor, ulong id, Data.FullAsset fullAsset, 
            UpdateUIDelegate actUpdateUI, uint timeout, ProgressMethodType progressMethod, 
            ErrorManager errorManager, FileSystem.IO fileSystem,
            Logger generalLogger, Logger networkLogger)
            : base(requestor, id, actUpdateUI, timeout, progressMethod, errorManager, 
            fileSystem, generalLogger, networkLogger)
        {
            _fullAsset = fullAsset;
        }
    }
}
