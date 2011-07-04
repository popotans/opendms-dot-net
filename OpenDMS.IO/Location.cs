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
            if (GetType() == typeof(Directory))
            {
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    path += Path.DirectorySeparatorChar.ToString();
                _path = path;
            }
            else
                _path = path;
        }

		#endregion Constructors 

		#region Properties (1) 

        public Directory Parent
        {
            get
            {
                if (System.IO.Directory.GetParent(_path) != null)
                    return new Directory(System.IO.Directory.GetParent(_path).FullName);
                else
                    return null;
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
