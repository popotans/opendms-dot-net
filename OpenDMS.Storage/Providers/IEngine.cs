using System;
using OpenDMS.IO;
using OpenDMS.Networking.Http;
using OpenDMS.Networking.Http.Methods;

namespace OpenDMS.Storage.Providers
{
    public interface IEngine
    {
        public delegate void ActionDelegate(EngineActionType actionType, bool willSendProgress);
        public delegate void ProgressDelegate(DirectionType direction, int packetSize, decimal sendPercentComplete, decimal receivePercentComplete);
        public delegate void CompletionDelegate(ICommandReply reply);
        public delegate void TimeoutDelegate();
        public delegate void ErrorDelegate(string message, Exception exception);
    }
}
