namespace Common
{
    public static class Globals
    {
        public const int DEFAULT_NETWORK_SENDTIMEOUT = 5000;
        public const int DEFAULT_NETWORK_RECEIVETIMEOUT = 5000;
        public const int DEFAULT_NETWORK_SENDBUFFERSIZE = 8192;
        public const int DEFAULT_NETWORK_RECEIVEBUFFERSIZE = 8192;

        private static int _network_SendTimeout = DEFAULT_NETWORK_SENDTIMEOUT;
        private static int _network_ReceiveTimeout = DEFAULT_NETWORK_RECEIVETIMEOUT;
        private static int _network_SendBufferSize = DEFAULT_NETWORK_SENDBUFFERSIZE;
        private static int _network_ReceiveBufferSize = DEFAULT_NETWORK_RECEIVEBUFFERSIZE;

        public static int Network_SendTimeout { get { return _network_SendTimeout; } }
        public static int Network_ReceiveTimeout { get { return _network_ReceiveTimeout; } }
        public static int Network_SendBufferSize { get { return _network_SendBufferSize; } }
        public static int Network_ReceiveBufferSize { get { return _network_ReceiveBufferSize; } }
    }
}
