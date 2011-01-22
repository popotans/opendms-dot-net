using System;
using System.IO;
using System.Collections.Generic;

namespace Common.FileSystem
{
    public class IO
    {
        private string _rootPath;
        private int _bufferSize;
        private Logger _logger;
        private List<FileState> _openStates;

        public IO(string rootPath)
            : this(rootPath, 40960, null)
        {
        }

        public IO(string rootPath, Logger logger)
            : this(rootPath, 40960, logger)
        {
        }

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

        public string GetFullFilePath(string relativePath)
        {
            return _rootPath + relativePath;
        }

        public IOStream Open(string relativePath, FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            return Open(relativePath, mode, access, share, options, _bufferSize, openedLocation);
        }

        /// <summary>
        /// Opens a resource
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <param name="options"></param>
        /// <param name="bufferSize"></param>
        /// <param name="openedLocation"></param>
        /// <returns></returns>
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
        /// Closes a IOStream, throwing an exception if the IOStream does not exist in this instance.
        /// </summary>
        /// <param name="stream"></param>
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

        public string[] GetFiles(string relativePath)
        {
            if (relativePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                CreateDirectoryPath(relativePath);
            else
                CreateDirectoryPath(relativePath + Path.DirectorySeparatorChar.ToString());

            return Directory.GetFiles(_rootPath + relativePath);
        }

        /// <summary>
        /// Determines if the resource exists on the underlying filesystem.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public bool ResourceExists(string relativePath)
        {
            return File.Exists(_rootPath + relativePath);
        }

        /// <summary>
        /// Determines if the resource is open by another process in this instance and supplies the
        /// FileState if found through the out state argument.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="state"></param>
        /// <returns></returns>
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
        /// Copies the source resource to the destination creating a new resource.
        /// </summary>
        /// <param name="relativeSourcePath"></param>
        /// <param name="relativeDestinationPath"></param>
        /// <returns></returns>
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
        /// Deletes a resource from the underlying filesystem if there is no conflicting usage by this
        /// instance.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
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

                // NOTE - there is no need to remove from _openStates as we checked to see if it
                // existed there earlier and it does not, else we would have failed earlier
                File.Delete(_rootPath + relativePath);
            }

            return true;
        }

        /// <summary>
        /// Deletes a list of files by their relative path, if there is an exception thrown by the 
        /// underlying filesystem during the actual deletion of the resource, an exception entry will
        /// be added to the list of exceptions argument.
        /// </summary>
        /// <param name="relativePaths"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
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
        /// <param name="relativePath"></param>
        public void CreateDirectoryPath(string relativePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_rootPath + relativePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(_rootPath + relativePath));
        }

        public ulong GetFileLength(string relativePath)
        {
            return (ulong)new FileInfo(_rootPath + relativePath).Length;
        }

        /// <summary>
        /// Computes the MD5 checksum value of the specified resource
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
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
        /// <param name="relativePath"></param>
        /// <param name="md5ToCompare"></param>
        /// <returns></returns>
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
