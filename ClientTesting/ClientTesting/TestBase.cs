using System;

namespace ClientTesting
{
    public abstract class TestBase
    {
        protected FrmMain _window;

        public TestBase(FrmMain window)
        {
            _window = window;
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
