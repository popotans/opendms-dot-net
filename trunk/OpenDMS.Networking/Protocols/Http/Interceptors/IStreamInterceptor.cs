using System;
using System.IO;

namespace OpenDMS.Networking.Protocols.Http.Interceptors
{
    public interface IStreamInterceptor : IDisposable
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        long Position { get; }
        int Read(byte[] buffer, int offset, int length);
        void ReadAsync(byte[] buffer, int offset, int length);
        void ReadToEndAsync();
        void Write(byte[] buffer, int offset, int length);
        void Flush();
        void Dispose();
    }
}
