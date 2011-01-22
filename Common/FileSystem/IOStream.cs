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
