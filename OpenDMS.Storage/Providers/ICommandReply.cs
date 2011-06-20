using System.Net;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers
{
    public interface ICommandReply
    {
        WebHeaderCollection Headers { get; }
        bool IsError { get; }
        string ErrorMessage { get; }
        string ResponseMessage { get; }
    }
}
