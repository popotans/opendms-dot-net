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
    /// <summary>
    /// An abstract class representing the base requirements of an inheriting Asset.
    /// </summary>
    public abstract class AssetBase
        : Common.NetworkPackage.DictionaryBase<string, object>
    {
        /// <summary>
        /// The <see cref="AssetState"/> describes the current state of the asset, actions on the asset should first check its state.
        /// </summary>
        protected AssetState _state;

        /// <summary>
        /// A reference to the <see cref="Database"/>.
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// Gets a Guid that provides a unique reference to an Asset.
        /// </summary>
        public Guid Guid
        {
            get { return (System.Guid)GetProperty("$guid"); }
            set { SetProperty("$guid", value); }
        }
        /// <summary>
        /// Gets a string value representing the Guid that provides a unique reference to an Asset.
        /// </summary>
        public string GuidString { get { return Guid.ToString("N"); } }
        /// <summary>
        /// Gets the state of the Asset.
        /// </summary>
        /// <value>
        /// The state of the Asset.
        /// </value>
        public AssetState AssetState { get { return _state; } }

        public AssetBase()
            : base("MetaAsset")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public AssetBase(Database cdb)
            : this(Guid.Empty, cdb)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        /// <param name="guid">The Guid providing a unique reference for this Asset.</param>
        /// <param name="cdb">A reference to the <see cref="Database"/>.</param>
        public AssetBase(Guid guid, Database cdb) 
            : base("MetaAsset")
        {
            Guid = guid;
            Database = cdb;
            _state = new AssetState(AssetState.Flags.None);
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected object GetProperty(string key)
        {
            if (ContainsKey("LockedBy"))
                return this["LockedBy"];
            else
                return null;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected void SetProperty(string key, object value)
        {
            if (ContainsKey("LockedBy"))
                this["LockedBy"] = value;
            else
                Add("LockedBy", value);
        }
    }
}
