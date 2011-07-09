using System;
using OpenDMS.Storage.Providers;

namespace StorageTesting
{
    public abstract class TestBase
    {
        protected FrmMain _window;
        protected IEngine _engine;
        protected IDatabase _db;

        public TestBase(FrmMain window, IEngine engine, IDatabase db)
        {
            _window = window;
            _engine = engine;
            _db = db;
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
