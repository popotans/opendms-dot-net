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

namespace Common.Data
{
    public class AssetState
    {
        public enum Flags
        {
            None = 0,
            LoadedFromLocal = 1,
            LoadedFromRemote = 2,
            InMemory = 4,
            MemoryDirty = 8,
            OnDisk = 16,
            DiskDirty = 32,
            CanTransfer
        }

        public Flags State { get; set; }

        public AssetState()
        {
            this.State = AssetState.Flags.None;
        }

        public AssetState(Flags flags)
        {
            this.State = flags;
        }

        public bool HasFlag(Flags flags)
        {
            return (State & flags) == flags;
        }
    }
}
