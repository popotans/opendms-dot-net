using System;
using System.Net;

namespace OpenDMS
{
    public class Settings
    {
        public static Settings Instance = new Settings();

        public IPEndPoint Host { get; set; }
        public string StorageLocation { get; set; }
        public long LeaseExpiration { get; set; }
        public int FileBufferSize { get; set; }

        public IPEndPoint SearchHost { get; set; }

        public Settings()
        {
            Host = new IPEndPoint(IPAddress.Parse("192.168.1.103"), 9160);
            StorageLocation = @"C:\DataStore\";
            LeaseExpiration = 900000; // 15 minutes
            FileBufferSize = 20480;
            SearchHost = new IPEndPoint(IPAddress.Parse("192.168.1.50"), 8080);
        }

    }
}
