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
    /// Represents the current state of a file resource on a file system.
    /// </summary>
    public class FileState
    {
        /// <summary>
        /// The full path of the resource
        /// </summary>
        public string FullPath;
        /// <summary>
        /// The relative path of the resource
        /// </summary>
        public string RelativePath;
        /// <summary>
        /// The mode with which the resource was accessed
        /// </summary>
        public FileMode Mode;
        /// <summary>
        /// The access rights to the resource
        /// </summary>
        public FileAccess Access;
        /// <summary>
        /// The sharing rights of the resource with other requests
        /// </summary>
        public FileShare Share;
        /// <summary>
        /// The options associated with the resource
        /// </summary>
        public FileOptions Options;
        /// <summary>
        /// Signifies where in the code the resource was initially accessed
        /// </summary>
        public string Owner;
        /// <summary>
        /// The IOStream that has access to the resource
        /// </summary>
        public IOStream Stream;
        /// <summary>
        /// When the resource was accessed
        /// </summary>
        public DateTime OpenedAt;
        /// <summary>
        /// The size of the buffer
        /// </summary>
        public int BufferSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fullPath">The full path of the resource</param>
        /// <param name="relativePath">The relative path of the resource</param>
        /// <param name="mode">The mode with which the resource was accessed</param>
        /// <param name="access">The access rights to the resource</param>
        /// <param name="share">The sharing rights of the resource with other requests</param>
        /// <param name="options">The options associated with the resource</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="owner">Signifies where in the code the resource was initially accessed</param>
        /// <param name="iostream">The IOStream that has access to the resource</param>
        public FileState(string fullPath, string relativePath, FileMode mode, FileAccess access, 
            FileShare share, FileOptions options, int bufferSize, string owner, IOStream iostream)
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            Mode = mode;
            Access = access;
            Share = share;
            Options = options;
            Owner = owner;
            OpenedAt = DateTime.Now;
            Stream = iostream;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Gets a string to be used for making a logging entry.
        /// </summary>
        /// <returns>A string to be used in a log.</returns>
        public string GetLogString()
        {
            return "FullPath=" + FullPath + "\r\n" +
                    "RelativePath=" + RelativePath + "\r\n" +
                    "Owner=" + Owner.ToString() + "\r\n" +
                    "OpenedAt=" + OpenedAt.ToString() + "\r\n" +
                    "Mode=" + Mode.ToString() + "\r\n" +
                    "Access=" + Access.ToString() + "\r\n" +
                    "Share=" + Share.ToString() + "\r\n" +
                    "Options=" + Options.ToString() + "\r\n" +
                    "BufferSize=" + BufferSize.ToString();
        }

        /// <summary>
        /// Determines if the argument <see cref="FileState"/> conflicts with this instance.
        /// </summary>
        /// <param name="state">The <see cref="FileState"/> to check.</param>
        /// <returns><c>True</c> if conflicting; otherwise, <c>false</c>.</returns>
        public bool DoesResourceConflict(FileState state)
        {
            if (state.RelativePath == RelativePath)
            {
                if ((state.Share & (FileShare)Access) != (FileShare)Access)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
