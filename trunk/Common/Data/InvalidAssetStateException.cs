using System;

namespace Common.Data
{
    public class InvalidAssetStateException 
        : Exception
    {
        public AssetState State;

        public InvalidAssetStateException(AssetState state)
            : base()
        {
            State = state;
        }

        public InvalidAssetStateException(AssetState state, string message)
            : base(message)
        {
            State = state;
        }

        public InvalidAssetStateException(AssetState state, string message, Exception innerException)
            : base(message, innerException)
        {
            State = state;
        }
    }
}
