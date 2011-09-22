using System;

namespace OpenDMS.Networking.Comm.Messages.Responses
{
    public interface IResponse
    {
        System.IO.Stream MakeStream();
    }
}
