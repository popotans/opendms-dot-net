using System;

namespace OpenDMS.Storage.Providers
{
    public interface IServer
    {
        string Protocol { get; }
        string Host { get; }
        int Port { get; }
        int Timeout { get; }
        int BufferSize { get; }

        Uri Uri { get; }

        IDatabase GetDatabase(string name, Security.DatabaseSessionManager sessionManager);
    }
}
