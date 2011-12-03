using System;
using Api = OpenDMS.Networking.Api;
using ClientLib = OpenDMS.ClientLibrary;

namespace ClientTesting
{
    public abstract class TestBase
    {
        protected FrmMain _window;
        protected ClientLib.Client _client;

        public TestBase(FrmMain window, ClientLib.Client client)
        {
            _window = window;
            _client = client;
        }

        public abstract void Test();

        public void Write(string str)
        {
            _window.Write(Timestamp() + str);
        }

        public void WriteLine(string str)
        {
            _window.WriteLine(Timestamp() + str);
        }

        public void Clear()
        {
            _window.Clear();
        }

        public string Timestamp()
        {
            return DateTime.Now.ToString("[d/M/y h:mm:ss.fffff tt] ");
        }
    }
}
