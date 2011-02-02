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
    /// Defines the asset as either a Meta (<see cref="MetaAsset"/>) or Data (<see cref="DataAsset"/>) asset.
    /// </summary>
    public class AssetType
    {
        /// <summary>
        /// An enumeration of types of Assets
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// An unknown asset type.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// A <see cref="MetaAsset"/>.
            /// </summary>
            Meta,
            /// <summary>
            /// A <see cref="DataAsset"/>.
            /// </summary>
            Data
        }

        /// <summary>
        /// A reference to an instantiated Meta <see cref="AssetType"/>.
        /// </summary>
        public static AssetType Meta = new AssetType(Type.Meta);
        /// <summary>
        /// A reference to an instantiated Data <see cref="AssetType"/>.
        /// </summary>
        public static AssetType Data = new AssetType(Type.Data);

        /// <summary>
        /// The <see cref="Type"/> of this instance.
        /// </summary>
        private Type _type;

        /// <summary>
        /// Gets or sets the <see cref="Type"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/>.
        /// </value>
        public Type Value
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// The system requires a root directory with seperate directories inside that for meta
        /// and data resources.  <see cref="VirtualPath"/> provides an accessor for the part of
        /// the path for those meta and data resources respectively.  This should be used anywhere
        /// the path must be specified to prevent errors in coding and help make the project
        /// maintainable.
        /// </summary>
        /// <example>
        /// This sample shows how to use the <see cref="VirtualPath"/> property.
        ///   <code>
        /// void A()
        /// {
        /// // VirtualPath does not include any starting or trailing directory seperators so you
        /// // must make sure and include them.
        /// string rootPath = @"C:\OpenDMS\";
        /// // Instantiate the IO class.
        /// Common.FileSystem.IO io = new Common.FileSystem.IO(rootPath);
        /// // This is already done by the IO constructor, but this is just for example purposes
        /// io.CreateDirectoryPath(rootPath + AssetType.Meta.VirtualPath);
        /// // The path 'C:\OpenDMS\meta' would now exist on the file system.
        /// }
        ///   </code>
        ///   </example>
        /// <remarks>
        /// Ensure that you include all starting or trailing directory seperators as
        /// VirtualPath does not include either (see example).
        /// </remarks>
        public string VirtualPath
        {
            get
            {
                switch (_type)
                {
                    case Type.Meta:
                        return "meta";
                    case Type.Data:
                        return "data";
                    default:
                        throw new ArgumentException("Invalid asset type.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetType"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public AssetType(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// Determines whether the specified <see cref="AssetType"/> is null or has a <see cref="Type"/> of Unknown.
        /// </summary>
        /// <param name="assetType"><see cref="AssetType"/> to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="AssetType"/> is null or has a <see cref="Type"/> of Unknown; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrUnknown(AssetType assetType)
        {
            if (assetType == null) return true;
            else if (assetType.Value == Type.Unknown) return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Argument cannot be null");

            if (obj.GetType() == typeof(AssetType))
                return ((AssetType)obj).Value == _type;
            else
                throw new ArgumentException("Invalid argument type.");
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _type.GetHashCode();
        }

        /// <summary>
        /// Gets the extension for this <see cref="AssetType"/> to be used when the Asset is saved to disk as a resource.  
        /// Note this only works with an <see cref="Type"/> of Meta.
        /// </summary>
        /// <returns>A string starting with a "." and then consisting of additional characters (e.g., .xml).</returns>
        /// <remarks>If the <see cref="Type"/> is not Meta then an exception will be thrown.  This is because 
        /// <see cref="DataAsset"/> extensions are maintained as their creating program made them and will 
        /// accordingly vary.</remarks>
        public string GetExtension()
        {
            switch (_type)
            {
                case Type.Meta:
                    return ".xml";
                case Type.Data:
                    throw new Exception("An AssetType.Data's extension cannot be determined by this class.");
                default:
                    throw new Exception("Unknown AssetType.");
            }
        }
    }
}
