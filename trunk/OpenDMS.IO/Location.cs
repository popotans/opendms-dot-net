using System.IO;

namespace OpenDMS.IO
{
    public abstract class Location
    {
        protected string _path = null;

        public Directory Parent
        {
            get
            {
                if (this == FileSystem.Instance.Root)
                    return null;

                return new Directory(System.IO.Directory.GetParent(_path).FullName);
            }
        }
        
        public Location(string path)
        {
            _path = path;
        }

        public override string ToString()
        {
            return _path;
        }

        public abstract void Delete();
    }
}
