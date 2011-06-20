
namespace OpenDMS.IO
{
    public class Singleton<T> where T : class, new()
    {
        protected bool _isInitialized = false;
        private static T _instance = default(T);

        public static T Instance
        {
            get
            {
                lock (typeof(T))
                {
                    if (_instance == default(T))
                        _instance = new T();
                }

                return _instance;
            }
        }
    }
}
