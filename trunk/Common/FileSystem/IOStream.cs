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
    public class IOStream
    {
        public Stream Stream { get { return _stream; } }
        public FileState State { get { return _state; } }

        private FileStream _stream;
        private FileState _state;

        public IOStream(string fullPath, string relativePath, FileMode mode, FileAccess access,
            FileShare share, FileOptions options, int bufferSize, string owner)
        {
            _stream = new FileStream(fullPath, mode, access, share, bufferSize, options);
            _state = new FileState(fullPath, relativePath, mode, access, share, options, 
                bufferSize, owner, this);
        }

        public string GetLogString()
        {
            return _state.GetLogString();
        }

        public void Close()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public int Read(byte[] buffer, int length)
        {
            return _stream.Read(buffer, 0, length);
        }

        public void Write(byte[] buffer, int length)
        {
            _stream.Write(buffer, 0, length);
        }

        public void CopyFrom(Stream source)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[_state.BufferSize];

            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                Write(buffer, bytesRead);
        }

        public void CopyTo(Stream destination)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[_state.BufferSize];

            while ((bytesRead = _stream.Read(buffer, 0, buffer.Length)) > 0)
                Write(buffer, bytesRead);
        }
    }
}
