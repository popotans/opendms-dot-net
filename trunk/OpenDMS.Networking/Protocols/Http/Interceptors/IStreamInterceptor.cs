using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public interface IStreamInterceptor : IDisposable
    {
        bool CanRead { get; }
        bool CanSeek { get; }
        bool CanWrite { get; }
        long Position { get; }
        int Read(byte[] buffer, int length);
        void Write(byte[] buffer, int length);
        void Flush();
        void Close();
        void Dispose();
    }
}
