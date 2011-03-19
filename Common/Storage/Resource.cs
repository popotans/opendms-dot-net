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
using Common.CouchDB;

namespace Common.Storage
{
    public sealed class Resource
    {
        private MetaAsset _metaAsset;
        private DataAsset _dataAsset;

        public MetaAsset MetaAsset { get { return _metaAsset; } }
        public DataAsset DataAsset { get { return _dataAsset; } }

        public Resource(Guid guid, Database cdb)
        {
            _metaAsset = new MetaAsset(guid, cdb);
            _dataAsset = new DataAsset(_metaAsset, cdb);
        }
    }
}
