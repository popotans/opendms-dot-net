using System.IO;

namespace OpenDMS.IO
{
    public abstract class Location
    {
		#region Fields (1) 

        protected string _path = null;

		#endregion Fields 

		#region Constructors (1) 

        public Location(string path)
        {
            _path = path;
        }

		#endregion Constructors 

		#region Properties (1) 

        public Directory Parent
        {
            get
            {
                if (this == FileSystem.Instance.Root)
                    return null;

                return new Directory(System.IO.Directory.GetParent(_path).FullName);
            }
        }

		#endregion Properties 

		#region Methods (3) 

		// Public Methods (3) 

        public abstract void Delete();

        public abstract bool Exists();

        public override string ToString()
        {
            return _path;
        }

		#endregion Methods 
    }
}
