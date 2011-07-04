using System;

namespace OpenDMS.Storage.Providers
{
    public interface IDatabase
    {
        string Name { get; }
        IServer Server { get; }
        Uri Uri { get; }
        Security.DatabaseSessionManager SessionManager { get; set; }
    }
}
