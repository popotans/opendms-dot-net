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
using System.Collections.Generic;

namespace Common.FileSystem
{
    /// <summary>
    /// Represents an object handling input/output to the local file system.
    /// </summary>
    public class IO
    {
        /// <summary>
        /// The path to the root directory to be used with this system.
        /// </summary>
        private string _rootPath;
        /// <summary>
        /// A positive Int32 value greater than 0 indicating the buffer size. For bufferSize values between one and eight, the actual buffer size is set to eight bytes.
        /// </summary>
        private int _bufferSize;
        /// <summary>
        /// A reference to the <see cref="Logger"/> that this instance should use to document events.
        /// </summary>
        private Logger _logger;
        /// <summary>
        /// A collection of open <see cref="FileState"/> objects.
        /// </summary>
        private List<FileState> _openStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="IO"/> class.
        /// </summary>
        /// <param name="rootPath">The path to the root directory to be used with this system.</param>
        public IO(string rootPath)
            : this(rootPath, 40960, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IO"/> class.
        /// </summary>
        /// <param name="rootPath">The path to the root directory to be used with this system.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public IO(string rootPath, Logger logger)
            : this(rootPath, 40960, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IO"/> class.
        /// </summary>
        /// <param name="rootPath">The path to the root directory to be used with this system.</param>
        /// <param name="bufferSize">A positive Int32 value greater than 0 indicating the buffer size. For bufferSize values between one and eight, the actual buffer size is set to eight bytes.</param>
        /// <param name="logger">A reference to the <see cref="Logger"/> that this instance should use to document events.</param>
        public IO(string rootPath, int bufferSize, Logger logger)
        {
            if(!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !rootPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                rootPath += Path.DirectorySeparatorChar.ToString();
            
            CreateDirectoryPath(rootPath + Data.AssetType.Meta.VirtualPath);
            CreateDirectoryPath(rootPath + Data.AssetType.Data.VirtualPath);

            _rootPath = rootPath;
            _bufferSize = bufferSize;
            _logger = logger;
            _openStates = new List<FileState>();
        }

        /// <summary>
        /// Gets the full file path given a relative path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The full file path.</returns>
        public string GetFullFilePath(string relativePath)
        {
            return _rootPath + relativePath;
        }

        /// <summary>
        /// Opens the file at the specified relative path.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="openedLocation">A string describing the location within the codebase where this method was called.</param>
        /// <returns>An <see cref="IOStream"/> allowing access to the specified resource.</returns>
        public IOStream Open(string relativePath, FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            return Open(relativePath, mode, access, share, options, _bufferSize, openedLocation);
        }

        /// <summary>
        /// Opens the file at the specified relative path.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="bufferSize">A positive Int32 value greater than 0 indicating the buffer size. For bufferSize values between one and eight, the actual buffer size is set to eight bytes.</param>
        /// <param name="openedLocation">A string describing the location within the codebase where this method was called.</param>
        /// <returns>An <see cref="IOStream"/> allowing access to the specified resource.</returns>
        public IOStream Open(string relativePath, FileMode mode, FileAccess access, FileShare share,
            FileOptions options, int bufferSize, string openedLocation)
        {
            FileState state;
            IOStream stream;
            List<FileState>.Enumerator en;

            lock (_openStates)
            {
                en = _openStates.GetEnumerator();

                // Check for conflicting share/access records
                while (en.MoveNext())
                {
                    state = (FileState)en.Current;
                    if (state.RelativePath == relativePath)
                    {
                        if ((state.Share & (FileShare)access) != (FileShare)access)
                        {
                            if (_logger != null)
                            {
                                _logger.Write(Logger.LevelEnum.Normal,
                                    "The resource is already open, the requested action conflicts with the " +
                                    "current accessability of the resource.\r\n" + state.GetLogString());
                            }
                            return null;
                        }
                    }
                }

                try
                {
                    stream = new IOStream(_rootPath + relativePath, relativePath, mode, access,
                        share, options, bufferSize, openedLocation);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal,
                            "An exception occurred while attempting to open a resource\r\n" +
                            Logger.ExceptionToString(e));
                    }
                    return null;
                }

                _openStates.Add(stream.State);

                if (_logger != null)
                {   // Log if facility is provided
                    _logger.Write(Logger.LevelEnum.Debug,
                        "Opened file handle\r\n" + stream.GetLogString());
                }
            }

            return stream;
        }

        /// <summary>
        /// Closes an <see cref="IOStream"/>, throwing an exception if the IOStream does not exist in this instance.
        /// </summary>
        /// <param name="stream">The <see cref="IOStream"/> to close.</param>
        public void Close(IOStream stream)
        {
            FileState state;
            List<FileState>.Enumerator en;

            lock (_openStates)
            {
                en = _openStates.GetEnumerator();

                // Check for match
                while (en.MoveNext())
                {
                    state = (FileState)en.Current;
                    if (state.Stream == stream)
                    {
                        state.Stream.Close();
                        state.Stream = null;

                        if (_logger != null)
                        {
                            _logger.Write(Logger.LevelEnum.Debug,
                                "Closed file handle\r\n" + stream.GetLogString());
                        }

                        _openStates.Remove(state);
                        state = null;
                        return;
                    }
                }
            }

            throw new IOException("The path is not open.");
        }

        /// <summary>
        /// Gets all files within a relative directory.
        /// </summary>
        /// <param name="relativePath">The relative directory.</param>
        /// <returns>An array of strings containing the full filepath for each file found within the relative directory.</returns>
        public string[] GetFiles(string relativePath)
        {
            if (relativePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                CreateDirectoryPath(relativePath);
            else
                CreateDirectoryPath(relativePath + Path.DirectorySeparatorChar.ToString());

            return Directory.GetFiles(_rootPath + relativePath);
        }

        /// <summary>
        /// Determines if the resource exists on the underlying file system.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <returns><c>True</c> when the file at the relative path exists; otherwise, <c>false</c>.</returns>
        public bool ResourceExists(string relativePath)
        {
            return File.Exists(_rootPath + relativePath.TrimStart(Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Determines if the relative path is a directory on the underlying file system.
        /// </summary>
        /// <param name="relativePath">The relative path of the directory.</param>
        /// <returns><c>True</c> when the directory at the relative path exists; otherwise, <c>false</c>.</returns>
        public bool DirectoryExists(string relativePath)
        {
            return Directory.Exists(_rootPath + relativePath);
        }

        /// <summary>
        /// Determines if the file is open by another process in this instance and supplies the
        /// <see cref="FileState"/> if found through the out <see cref="FileState"/> argument.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <param name="state">The <see cref="FileState"/> of the file if open by another process.</param>
        /// <returns><c>True</c> if the file is open; otherwise, <c>false</c>.</returns>
        public bool HandleExists(string relativePath, out FileState state)
        {
            List<FileState>.Enumerator en;
            state = null;

            lock (_openStates)
            {
                en = _openStates.GetEnumerator();

                while (en.MoveNext())
                {
                    if (en.Current.RelativePath == relativePath)
                    {
                        state = en.Current;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Copies the specified source stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="destinationStream">The destination stream.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Copy(IOStream sourceStream, IOStream destinationStream)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[_bufferSize];

            lock (_openStates)
            {
                if (_openStates.Contains(sourceStream.State))
                {
                    _openStates.Remove(sourceStream.State);
                    return false;
                }

                if (_openStates.Contains(destinationStream.State))
                {
                    _openStates.Remove(sourceStream.State);
                    _openStates.Remove(destinationStream.State);
                    return false;
                }
            }

            try
            {
                while ((bytesRead = sourceStream.Read(buffer, buffer.Length)) > 0)
                    destinationStream.Write(buffer, bytesRead);
            }
            catch (Exception e)
            {
                if (_logger != null)
                {
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while attempting to " +
                        "copy the resource, the destination file will be deleted.\r\n" +
                        Logger.ExceptionToString(e));
                }

                // Close the resources
                destinationStream.Close();
                sourceStream.Close();

                // Remove handles
                if (_openStates.Contains(sourceStream.State))
                    _openStates.Remove(sourceStream.State);
                if (_openStates.Contains(destinationStream.State))
                    _openStates.Remove(destinationStream.State);

                return false;
            }

            Close(destinationStream);
            Close(sourceStream);

            return true;
        }

        /// <summary>
        /// Renames a directory.
        /// </summary>
        /// <param name="relativeSourcePath">The relative source path.</param>
        /// <param name="newDirName">New name of the directory.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool RenameDirectory(string relativeSourcePath, string newDirName)
        {
            string destinationRelativePath = relativeSourcePath.TrimEnd(new char[] { Path.DirectorySeparatorChar });

            destinationRelativePath = relativeSourcePath.Substring(0, relativeSourcePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            destinationRelativePath += newDirName;

            lock (_openStates)
            {
                if (_openStates.Count > 0)
                {
                    if (_logger != null)
                        _logger.Write(Logger.LevelEnum.Normal, "Cannot rename directory as their are resources currently open.");
                    return false;
                }

                try
                {
                    Directory.Move(_rootPath.TrimEnd(Path.DirectorySeparatorChar) + relativeSourcePath,
                        _rootPath.TrimEnd(Path.DirectorySeparatorChar) + destinationRelativePath);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Rename directory failed with the following exception:\r\n" +
                            Logger.ExceptionToString(e));
                    }
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Renames the source resource to the destination.
        /// </summary>
        /// <param name="relativeSourcePath">The relative source path.</param>
        /// <param name="newFileName">New name of the file, only provide the filename, not the any path information.</param>
        /// <param name="overwrite">If set to <c>true</c> and the destination file exists it will be overwritten.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Rename(string relativeSourcePath, string newFileName, bool overwrite)
        {
            FileState state;
            string destinationRelativePath = relativeSourcePath.Substring(0, relativeSourcePath.LastIndexOf(Path.DirectorySeparatorChar) +1);
            destinationRelativePath += newFileName;

            //if (!relativeSourcePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            //    relativeSourcePath += Path.DirectorySeparatorChar.ToString();
            //if (!destinationRelativePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            //    destinationRelativePath += Path.DirectorySeparatorChar.ToString();

            lock (_openStates)
            {
                // Check for conflicting usage
                if (HandleExists(relativeSourcePath, out state))
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Resource " + relativeSourcePath +
                            "cannot be renamed as it is " +
                            "open by another process, the owning process information follows:\r\n" +
                            state.GetLogString());
                    }
                    return false;
                }

                // Make sure the source exists
                if (!ResourceExists(relativeSourcePath))
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Resource " + relativeSourcePath +
                            " does not exist.");
                    }
                    return false;
                }

                // Make sure the destination does not exist if overwrite is false
                if (!overwrite && ResourceExists(destinationRelativePath))
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Rename failed because the destination relative path of \"" +
                            destinationRelativePath + "\" already exists and overwrite was set to false.");
                    }
                    return false;
                }

                try
                {
                    File.Move(_rootPath.TrimEnd(Path.DirectorySeparatorChar) + relativeSourcePath, 
                        _rootPath.TrimEnd(Path.DirectorySeparatorChar) + destinationRelativePath);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Rename failed with the following exception:\r\n" +
                            Logger.ExceptionToString(e));
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Copies the source resource to the destination creating a new resource.
        /// </summary>
        /// <param name="relativeSourcePath">The source file.</param>
        /// <param name="relativeDestinationPath">The destination file.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool Copy(string relativeSourcePath, string relativeDestinationPath)
        {
            IOStream source, dest;
            int bytesRead = 0;
            byte[] buffer = new byte[_bufferSize];

            lock (_openStates)
            {
                if ((source = Open(relativeSourcePath, FileMode.Open, FileAccess.Read, FileShare.None,
                    FileOptions.None, _bufferSize, "Common.FileSystem.IO.Copy()")) == null)
                {
                    if (_openStates.Contains(source.State))
                        _openStates.Remove(source.State);
                    return false;
                }

                if ((dest = Open(relativeDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
                    FileOptions.SequentialScan, _bufferSize, "Common.FileSystem.IO.Copy()")) == null)
                {
                    if (_openStates.Contains(dest.State))
                        _openStates.Remove(dest.State);
                    if (_openStates.Contains(source.State))
                        _openStates.Remove(source.State);
                    if (!Delete(relativeDestinationPath))
                        throw new IOException("An unexpected exception occurred while attempting to delete the destination resource after failing to open the resource for writing.");
                    return false;
                }
            }

            try
            {
                while ((bytesRead = source.Read(buffer, buffer.Length)) > 0)
                    dest.Write(buffer, bytesRead);
            }
            catch (Exception e)
            {
                if (_logger != null)
                {
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while attempting to " +
                        "copy the resource, the destination file will be deleted.\r\n" +
                        Logger.ExceptionToString(e));
                }

                // Close the resources
                dest.Close();
                source.Close();

                // Remove handles
                if (_openStates.Contains(source.State))
                    _openStates.Remove(source.State);
                if (_openStates.Contains(dest.State))
                    _openStates.Remove(dest.State);

                // Delete the destination since this failed
                Delete(relativeDestinationPath);

                return false;
            }

            Close(dest);
            Close(source);

            return true;
        }

        /// <summary>
        /// Gets the <see cref="DirectoryInfo"/> object for the relative path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>A <see cref="DirectoryInfo"/> for the relative path.</returns>
        public DirectoryInfo GetDirectoryInfo(string relativePath)
        {
            return new DirectoryInfo(_rootPath.TrimEnd(Path.DirectorySeparatorChar) + relativePath);
        }

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns><c>True</c> if successful; otherwise, <c>false</c>.</returns>
        public bool DeleteDirectory(string relativePath)
        {
            relativePath = relativePath.Trim(Path.DirectorySeparatorChar);

            if (relativePath == Data.AssetType.Meta.VirtualPath ||
                relativePath == Data.AssetType.Data.VirtualPath ||
                relativePath == "settings")
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Could not delete the directory at " +
                        relativePath + " because it is a required directory.");
                return false;
            }

            try
            {
                Directory.Delete(_rootPath.TrimEnd(Path.DirectorySeparatorChar) + relativePath);
            }
            catch (Exception e)
            {
                if (_logger != null)
                {
                    _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while attempting " +
                        "to delete the directory from the underlying file system.\r\n" +
                        Logger.ExceptionToString(e));
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes a file from the local file system if there is no conflicting usage by this
        /// <see cref="IO"/> object.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <returns><c>True</c> if the file was successfully removed or if it does not exist; otherwise,
        /// <c>false</c>.</returns>
        public bool Delete(string relativePath)
        {
            FileState state;

            lock (_openStates)
            {
                // Check for conflicting usage
                if(HandleExists(relativePath, out state))
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Normal, "Resource " + relativePath + 
                            "cannot be deleted as it is " +
                            "open by another process, the owning process information follows:\r\n" +
                            state.GetLogString());
                    }
                    return false;
                }

                if (!ResourceExists(relativePath))
                {
                    if (_logger != null)
                    {
                        _logger.Write(Logger.LevelEnum.Debug, "Resource " + relativePath +
                            "does not exist.");
                    }
                    return true;
                }


                // Sometimes, due to multi-threading, we can be running a "change" process received from the
                // FileSystemWatcher or something else.  Accordingly, we need to make multiple attempts, we
                // will do 5 trys.
                for (int i = 0; i < 5; i++)
                {
                    // NOTE - there is no need to remove from _openStates as we checked to see if it
                    // existed there earlier and it does not, else we would have failed earlier
                    try
                    {
                        File.Delete(_rootPath + relativePath);
                        break;
                    }
                    catch (System.IO.IOException e)
                    {
                        if (_logger != null)
                        {
                            _logger.Write(Logger.LevelEnum.Debug, "Resource " + relativePath +
                                "does not exist.");
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Deletes a list of files by their relative path, if there is an exception thrown by the 
        /// underlying file system during the actual deletion of the resource, an exception entry will
        /// be added to the list of exceptions argument.
        /// </summary>
        /// <param name="relativePaths">The relative paths for all files to remove.</param>
        /// <param name="exceptions">A collection of exceptions thrown during removal of files.</param>
        /// <returns><c>True</c> if all successful; otherwise, <c>false</c>.</returns>
        public bool DeleteMultipleFiles(List<string> relativePaths, out List<Exception> exceptions)
        {
            // This attempts to be all or nothing - meaning it checks our system, but we cannot
            // test against the actual underlying filesystem
            FileState state;
            exceptions = new List<Exception>();

            lock (_openStates)
            {
                // Check for conflicting usage
                for (int i = 0; i < relativePaths.Count; i++)
                {
                    if(HandleExists(relativePaths[i], out state))
                    {
                        if (_logger != null)
                        {
                            _logger.Write(Logger.LevelEnum.Normal, "Resource " + relativePaths[i] +
                                "cannot be deleted as it is " +
                                "open by another process, the owning process information follows:\r\n" +
                                state.GetLogString());
                        }
                        return false;
                    }
                }

                // Nothing hit on _openStates -> check the underlying FS to ensure all exist, if it does
                // not exist, we still proceed, we just log it.
                for (int i = 0; i < relativePaths.Count; i++)
                {
                    if (!ResourceExists(relativePaths[i]))
                    {
                        if (_logger != null)
                        {
                            _logger.Write(Logger.LevelEnum.Debug, "Resource " + relativePaths[i] +
                                "does not exist.");
                        }
                    }
                }

                // Run deletes                
                for (int i = 0; i < relativePaths.Count; i++)
                {
                    try
                    {
                        File.Delete(relativePaths[i]);
                    }
                    catch (Exception e)
                    {
                        if (_logger != null)
                        {
                            _logger.Write(Logger.LevelEnum.Normal, "An exception occurred while attempting " +
                                "to delete the resource from the underlying filesystem.\r\n" +
                                Logger.ExceptionToString(e));
                        }
                        exceptions.Add(e);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates all directories needed to establish relativePath
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        public void CreateDirectoryPath(string relativePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_rootPath + relativePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_rootPath + relativePath));
        }

        /// <summary>
        /// Gets the length of the file in bytes.
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <returns></returns>
        public ulong GetFileLength(string relativePath)
        {
            return (ulong)new FileInfo(_rootPath + relativePath).Length;
        }

        /// <summary>
        /// Gets the relative path from full path.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns>A string representing the relative path.</returns>
        public string GetRelativePathFromFullPath(string fullPath)
        {
            if (fullPath.Contains(_rootPath))
                return fullPath.Replace(_rootPath, @"\");
            else
                throw new ArgumentException("The argument does not contain the root path of \"" + _rootPath + "\"");
        }

        /// <summary>
        /// Computes the MD5 checksum value of the specified resource
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <returns>A string representing the MD5 value.</returns>
        public string ComputeMd5(string relativePath)
        {
            IOStream stream;
            System.Security.Cryptography.MD5 md5;
            byte[] data;
            string output = "";

            md5 = System.Security.Cryptography.MD5.Create();

            if (!ResourceExists(relativePath))
            {
                if (_logger != null)
                {
                    _logger.Write(Logger.LevelEnum.Normal, "Cannot compute the MD5 value of the specified " +
                        "resource as it does not exist, resource: " + relativePath);
                }
                throw new FileNotFoundException("File not found", _rootPath + relativePath);
            }

            stream = Open(relativePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None,
                _bufferSize, "Common.FileSystem.IO.ComputeMd5()");

            // Compute
            data = md5.ComputeHash(stream.Stream);

            // Close
            Close(stream);

            for (int i = 0; i < data.Length; i++)
                output += data[i].ToString("x2");

            return output;
        }

        /// <summary>
        /// Checks to ensure matching MD5 checksum values for the argument path and the argument md5 value
        /// </summary>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <param name="md5ToCompare">The MD5 value to compare with the MD5 value of this instance.</param>
        /// <returns><c>True</c> if the values match; otherwise, <c>false</c>.</returns>
        public bool VerifyMd5(string relativePath, string md5ToCompare)
        {
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (!ResourceExists(relativePath))
            {
                if (_logger != null)
                {
                    _logger.Write(Logger.LevelEnum.Normal, "Cannot compute the MD5 value of the specified " +
                        "resource as it does not exist, resource: " + relativePath);
                }
                throw new FileNotFoundException("File not found", _rootPath + relativePath);
            }

            if (comparer.Compare(ComputeMd5(relativePath), md5ToCompare) != 0)
                return false;

            return true;
        }
    }
}
