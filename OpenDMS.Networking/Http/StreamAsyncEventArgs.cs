using System;

namespace OpenDMS.Networking.Http
{
    public class StreamAsyncEventArgs : EventArgs, IDisposable
    {
        public delegate void EventDelegate(StreamAsyncEventArgs e);
        public EventDelegate Complete { get; set; }

        public byte[] Buffer { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }
        public object UserToken { get; set; }

        public StreamAsyncEventArgs()
        {
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
