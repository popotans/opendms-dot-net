
namespace OpenDMS.IO
{
    public class Singleton<T> where T : class, new()
    {
		#region Fields (2) 

        private static T _instance = default(T);
        protected bool _isInitialized = false;

		#endregion Fields 

		#region Properties (1) 

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

		#endregion Properties 

		#region Methods (1) 

		// Public Methods (1) 

        public void CheckInitialization()
        {
            if (!_isInitialized) throw new OpenDMS.IO.NotInitializedException("The " + GetType().FullName + " has not been initialized.");
        }

		#endregion Methods 
    }
}
