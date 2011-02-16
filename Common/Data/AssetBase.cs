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
    /// <summary>
    /// An abstract class representing the base requirements of an inheriting Asset.
    /// </summary>
    public abstract class AssetBase
    {
        /// <summary>
        /// A Guid that provides a unique reference to an Asset.
        /// </summary>
        protected Guid _guid;
        /// <summary>
        /// The <see cref="AssetType"/> of this instance (Meta or Data).
        /// </summary>
        protected AssetType _assetType;
        /// <summary>
        /// A reference to the <see cref="FileSystem.ResourceBase"/> giving this Asset access to the filesystem.
        /// </summary>
        protected FileSystem.ResourceBase _resource;
        /// <summary>
        /// The <see cref="AssetState"/> describes the current state of the asset, actions on the asset should first check its state.
        /// </summary>
        protected AssetState _state;

        /// <summary>
        /// Gets a Guid that provides a unique reference to an Asset.
        /// </summary>
        public Guid Guid { get { return _guid; } }
        /// <summary>
        /// Gets a string value representing the Guid that provides a unique reference to an Asset.
        /// </summary>
        public string GuidString { get { return Guid.ToString("N"); } }
        /// <summary>
        /// Gets the type of the asset (Meta or Data).
        /// </summary>
        /// <value>
        /// The type of the Asset (Meta or Data).
        /// </value>
        public AssetType AssetType { get { return _assetType; } }
        /// <summary>
        /// Gets the state of the Asset.
        /// </summary>
        /// <value>
        /// The state of the Asset.
        /// </value>
        public AssetState AssetState { get { return _state; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        public AssetBase() : 
            this(Guid.Empty, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> of this instance (Meta or Data).</param>
        public AssetBase(AssetType assetType)
            : this(Guid.Empty, assetType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBase"/> class.
        /// </summary>
        /// <param name="guid">The Guid providing a unique reference for this Asset.</param>
        /// <param name="assetType">The <see cref="AssetType"/> of this instance (Meta or Data).</param>
        public AssetBase(Guid guid, AssetType assetType)
        {
            _guid = guid;
            _assetType = assetType;
            _resource = null;
            _state = new AssetState(AssetState.Flags.None);
        }

        /// <summary>
        /// Checks the underlying file system to determine if this Asset's corresponding Resource exists.  Note
        /// that just because a resource exists representing this asset does not necessarily mean that it holds
        /// the same data.
        /// </summary>
        /// <returns>True if the resource exists on the file system, false otherwise</returns>
        /// <remarks>
        /// Just because a resource exists does not mean that is contains the same data as the instance of the Asset.
        /// </remarks>
        /// <example>
        /// This sample shows how to call the <see cref="ResourceExistsOnFilesystem"/> method.  This code
        /// assumes that _asset has been previously and properly defined and instantiated as either a 
        /// <see cref="MetaAsset"/> or a <see cref="DataAsset"/>.
        /// <code>
        /// void A()
        /// {
        ///     // Once again, _asset is assumed already instantiated as either a <see cref="MetaAsset"/> 
        ///     // or a <see cref="DataAsset"/>.
        ///     
        ///     if(!_asset.ResourceExistsOnFilesystem())
        ///     {
        ///         MessageBox.Show("A resource does not exist on the file system representing this asset.");
        ///     }
        ///     else
        ///     {
        ///         MessageBox.Show("A resource exists on the file system representing this asset.");
        ///     }
        /// }    
        /// </code>
        /// </example>
        public bool ResourceExistsOnFilesystem()
        {
            return _resource.ExistsOnFilesystem();
        }
    }
}
