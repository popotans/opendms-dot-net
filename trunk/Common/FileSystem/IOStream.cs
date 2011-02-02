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
    /// Represents a pathway to a file on the local file system.
    /// </summary>
    public class IOStream
    {
        /// <summary>
        /// Gets a reference to the underlying stream.
        /// </summary>
        public Stream Stream { get { return _stream; } }
        /// <summary>
        /// Gets the <see cref="FileState"/> of the underlying file.
        /// </summary>
        public FileState State { get { return _state; } }

        /// <summary>
        /// A reference to the underlying stream.
        /// </summary>
        private FileStream _stream;
        /// <summary>
        /// A reference to the underlying file's state.
        /// </summary>
        private FileState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="IOStream"/> class.
        /// </summary>
        /// <param name="fullPath">The full path of the file.</param>
        /// <param name="relativePath">The relative path of the file.</param>
        /// <param name="mode">The <see cref="FileMode"/>.</param>
        /// <param name="access">The <see cref="FileAccess"/>.</param>
        /// <param name="share">The <see cref="FileShare"/>.</param>
        /// <param name="options">The <see cref="FileOptions"/>.</param>
        /// <param name="bufferSize">A positive Int32 value greater than 0 indicating the buffer size. For bufferSize values between one and eight, the actual buffer size is set to eight bytes.</param>
        /// <param name="owner">A string describing the location within the codebase where this method was called.</param>
        public IOStream(string fullPath, string relativePath, FileMode mode, FileAccess access,
            FileShare share, FileOptions options, int bufferSize, string owner)
        {
            _stream = new FileStream(fullPath, mode, access, share, bufferSize, options);
            _state = new FileState(fullPath, relativePath, mode, access, share, options, 
                bufferSize, owner, this);
        }

        /// <summary>
        /// Gets a string for use in a log entry.
        /// </summary>
        /// <returns>A string for use in a log entry.</returns>
        public string GetLogString()
        {
            return _state.GetLogString();
        }

        /// <summary>
        /// Closes this <see cref="IOStream"/>.- Use <see cref="IO.Close"/> unless 
        /// you are sure you should use this.
        /// </summary>
        /// <remarks>This should never be used outside of the <see cref="IO"/> class.</remarks>
        public void Close()
        {
            _stream.Close();
            _stream.Dispose();
        }

        /// <summary>
        /// Reads a number of bytes from the stream into the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to fill with data.</param>
        /// <param name="length">The quantity of bytes to read from the stream.</param>
        /// <returns></returns>
        public int Read(byte[] buffer, int length)
        {
            return _stream.Read(buffer, 0, length);
        }

        /// <summary>
        /// Writes a number of bytes from the buffer to the stream
        /// </summary>
        /// <param name="buffer">The buffer supplying data.</param>
        /// <param name="length">The quantity of bytes to write from the buffer to the stream.</param>
        public void Write(byte[] buffer, int length)
        {
            _stream.Write(buffer, 0, length);
        }

        /// <summary>
        /// Copies all data from the source stream to the underlying stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        public void CopyFrom(Stream source)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[_state.BufferSize];

            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                Write(buffer, bytesRead);
        }

        /// <summary>
        /// Copies the underlying stream to the destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        public void CopyTo(Stream destination)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[_state.BufferSize];

            while ((bytesRead = _stream.Read(buffer, 0, buffer.Length)) > 0)
                Write(buffer, bytesRead);
        }
    }
}
