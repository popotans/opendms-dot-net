using System;

namespace OpenDMS.Networking.Protocols
{
    public class StreamAsyncEventArgs : EventArgs, IDisposable
    {
        #region Constructors (1)

        public StreamAsyncEventArgs()
        {
        }

        #endregion Constructors

        #region Properties (5)

        public byte[] Buffer { get; private set; }

        public EventDelegate Complete { get; set; }

        public int Count { get; private set; }

        public int Offset { get; private set; }

        public object UserToken { get; set; }

        #endregion Properties

        #region Delegates and Events (1)

        // Delegates (1) 

        public delegate void EventDelegate(StreamAsyncEventArgs e);

        #endregion Delegates and Events

        #region Methods (2)

        // Public Methods (2) 

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        #endregion Methods 
    }
}
