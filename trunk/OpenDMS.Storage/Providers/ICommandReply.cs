using System.Net;
using Http = OpenDMS.Networking.Protocols.Http;

namespace OpenDMS.Storage.Providers
{
    public interface ICommandReply
    {
        Http.Message.HeaderCollection Headers { get; }
        bool IsError { get; }
        string ErrorMessage { get; }
        string ResponseMessage { get; }
    }
}
