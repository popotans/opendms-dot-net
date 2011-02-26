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
using System.IO;

namespace Common.FileSystem
{
    /// <summary>
    /// A class representing the base requirements of an inheriting Resource.
    /// </summary>
    public class ResourceBase
    {
        /// <summary>
        /// A reference to a <see cref="IO"/> object.
        /// </summary>
        private IO _fileSystem;
        /// <summary>
        /// A <see cref="Guid"/> that provides a unique reference to a Resource.
        /// </summary>
        private Guid _guid;
        /// <summary>
        /// The <see cref="ResourceType"/> of this instance (Meta or Data).
        /// </summary>
        private ResourceType _type;
        /// <summary>
        /// The extension of the file on the file system.
        /// </summary>
        private string _extension;

        /// <summary>
        /// <c>True</c> when the resource is in an open state; otherwise, <c>false</c>.
        /// </summary>
        private bool _isOpen;
        /// <summary>
        /// The <see cref="IOStream"/> allowing access to the Resource.
        /// </summary>
        private IOStream _stream;

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> that provides a unique reference to a Resource.
        /// </summary>
        /// <value>
        /// The <see cref="Guid"/>.
        /// </value>
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        /// <summary>
        /// Gets or sets the extension of the file on the file system.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public string Extension
        {
            get { return _extension; }
            set { _extension = value; }
        }

        /// <summary>
        /// Gets the directory relative to the root directory where this Resource belongs.
        /// </summary>
        public string RelativeDirectory
        {
            get
            {
                switch (_type)
                {
                    case ResourceType.Meta:
                        return "meta" + Path.DirectorySeparatorChar.ToString();
                    case ResourceType.Data:
                        return "data" + Path.DirectorySeparatorChar.ToString();
                    default:
                        throw new IOException("Invalid ResourceType");
                }
            }
        }

        /// <summary>
        /// Gets the filepath relative to the root directory where this Resource belongs.
        /// </summary>
        public string RelativeFilepath
        {
            get 
            {
                return RelativeDirectory + _guid.ToString("N") + _extension;
            }
        }

        /// <summary>
        /// Gets the length of the file in bytes.
        /// </summary>
        /// <value>
        /// The length of the file in bytes.
        /// </value>
        public ulong FileLength
        {
            get
            {
                return _fileSystem.GetFileLength(RelativeFilepath);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBase"/> class.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> that provides a unique reference to a Resource.</param>
        /// <param name="type">The <see cref="ResourceType"/> of this instance (Meta or Data).</param>
        /// <param name="extension">The extension of the file on the file system.</param>
        /// <param name="fileSystem">A reference to a <see cref="IO"/> object.</param>
        public ResourceBase(Guid guid, ResourceType type, string extension, IO fileSystem)
        {
            if(type == ResourceType.Unknown)
                throw new ArgumentException("Argument type must be Meta or Data.");

            _fileSystem = fileSystem;
            _guid = guid;
            _type = type;
            _stream = null;
            _isOpen = false;

            if (type == ResourceType.Meta)
                _extension = ".xml";
            else
                _extension = extension;
        }

        /// <summary>
        /// Gets an exclusive read stream on a Resource.
        /// </summary>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        public IOStream GetExclusiveReadStream(string openedLocation)
        {
            return GetStreamInternal(FileMode.Open, FileAccess.Read, FileShare.None,
                 FileOptions.None, "Common.FileSystem.FSObject.GetExclusiveReadStream() from " +
                 openedLocation);
        }

        /// <summary>
        /// Gets the exclusive write stream on a Resource.
        /// </summary>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        public IOStream GetExclusiveWriteStream(string openedLocation)
        {
            return GetStreamInternal(FileMode.Create, FileAccess.Write, FileShare.None,
                FileOptions.None, "Common.FileSystem.FSObject.GetExclusiveWriteStream() from " + 
                openedLocation);
        }

        /// <summary>
        /// Gets the exclusive write stream on a Resource using version scheme naming.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        public IOStream GetExclusiveWriteStreamUsingVersionScheme(UInt64 version,
            string openedLocation)
        {
            return GetStreamInternalUsingVersionScheme(version, FileMode.Open, FileAccess.Read, 
                FileShare.None, FileOptions.None, 
                "Common.FileSystem.FSObject.GetExclusiveWriteStream() from " +
                openedLocation);
        }

        /// <summary>
        /// Copies the current version to the specified version using version scheme naming.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        public bool CopyCurrentToVersionScheme(UInt64 version, string openedLocation)
        {
            return _fileSystem.Copy(RelativeFilepath,
                RelativeDirectory + Guid.ToString("N") + "_" + version.ToString() + _extension);
        }

        /// <summary>
        /// Copies the current version to the specified relative destination.
        /// </summary>
        /// <param name="destinationRelativeFilePath">The root relative destination file path.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool CopyToRelativeFilePath(string destinationRelativeFilePath)
        {
            if (!_fileSystem.DirectoryExists(destinationRelativeFilePath))
                _fileSystem.CreateDirectoryPath(destinationRelativeFilePath);
            return _fileSystem.Copy(RelativeFilepath, destinationRelativeFilePath);
        }

        /// <summary>
        /// Copies the specified relative source file path to the current version.
        /// </summary>
        /// <param name="sourceRelativeFilePath">The source relative file path.</param>
        /// <returns></returns>
        public bool CopyFromRelativeFilePath(string sourceRelativeFilePath)
        {
            return _fileSystem.Copy(sourceRelativeFilePath, RelativeFilepath);
        }

        /// <summary>
        /// Gets an <see cref="IOStream"/> using the specified parameters.
        /// </summary>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        public IOStream GetStream(FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            return GetStreamInternal(mode, access, share, options, 
                "Common.FileSystem.FSObject.GetStream() from " + openedLocation);
        }

        /// <summary>
        /// Gets an <see cref="IOStream"/> using the specified parameters using version scheme naming.
        /// </summary>
        /// <param name="version">The version number.</param>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        private IOStream GetStreamInternalUsingVersionScheme(UInt64 version, FileMode mode, 
            FileAccess access, FileShare share, FileOptions options, string openedLocation)
        {
            if (_stream != null)
                throw new IOException("The resource's stream is already open.");

            CreateContainingDirectory();

            try
            {
                _stream = _fileSystem.Open(RelativeDirectory + 
                    Guid.ToString("N") + "_" + version.ToString() + Extension, 
                    mode, access, share, options, openedLocation);
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to open the resource.", e);
            }

            if (_stream == null)
            {
                Logger.General.Error("Failed to open the resource.");
                return _stream;
            }

            // The fact that it was opened will be logged through the FileSystem.IO object
            _isOpen = true;
            return _stream;
        }

        /// <summary>
        /// Gets an <see cref="IOStream"/> using the specified parameters and can only be used from within this class.
        /// </summary>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="openedLocation">The location where the Resource was opened in the code.</param>
        /// <returns>An <see cref="IOStream"/> for Resource access.</returns>
        private IOStream GetStreamInternal(FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            if (_stream != null)
                throw new IOException("The resource's stream is already open.");

            CreateContainingDirectory();

            try
            {
                _stream = _fileSystem.Open(RelativeFilepath, mode, access, share,
                    options, openedLocation);
            }
            catch (Exception e)
            {
                Logger.General.Error("Failed to open the resource.", e);
            }

            if (_stream == null)
            {
                Logger.General.Error("Failed to open the resource.");
                return _stream;
            }

            // The fact that it was opened will be logged through the FileSystem.IO object
            _isOpen = true;
            return _stream;
        }

        /// <summary>
        /// Closes the <see cref="IOStream"/> for this Resource.
        /// </summary>
        public void CloseStream()
        {
            if (_isOpen)
            {
                _fileSystem.Close(_stream);
                _stream = null;
                _isOpen = false;
            }
        }

        /// <summary>
        /// Creates the directory path containing the <see cref="RelativeFilepath"/>
        /// </summary>
        public void CreateContainingDirectory()
        {
            _fileSystem.CreateDirectoryPath(RelativeDirectory);
        }

        /// <summary>
        /// Checks if a Resource exists at the <see cref="RelativeFilepath"/>.
        /// </summary>
        /// <returns><c>True</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ExistsOnFilesystem()
        {
            return _fileSystem.ResourceExists(RelativeFilepath);
        }

        /// <summary>
        /// Renames the this Resource to the new name.
        /// </summary>
        /// <param name="newGuid">The new Guid.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The new name is not a full path, it is simply a name including extension.
        /// </remarks>
        public bool Rename(Guid newGuid)
        {
            string oldName = RelativeFilepath;
            _guid = newGuid;
            return _fileSystem.Rename("\\" + oldName, _guid.ToString("N") + _extension, false);
        }

        /// <summary>
        /// Deletes this Resource from the file system.
        /// </summary>
        public void DeleteFromFilesystem()
        {
            _fileSystem.Delete(RelativeFilepath);
        }

        /// <summary>
        /// Computes the MD5 checksum of this Resource.
        /// </summary>
        /// <returns>A string representation of the MD5 checksum value.</returns>
        public string ComputeMd5()
        {
            return _fileSystem.ComputeMd5(RelativeFilepath);
        }

        /// <summary>
        /// Verifies the MD5.
        /// </summary>
        /// <param name="md5ToCompare">The MD5 to compare.</param>
        /// <returns></returns>
        public bool VerifyMd5(string md5ToCompare)
        {
            return _fileSystem.VerifyMd5(RelativeFilepath, md5ToCompare);
        }
    }
}
