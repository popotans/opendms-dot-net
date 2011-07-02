using System.Collections.Generic;

namespace OpenDMS.IO
{
    public class Directory : Location
    {
		#region Constructors (1) 

        public Directory(string path)
            : base(System.IO.Path.GetDirectoryName(path))
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

		#endregion Constructors 

		#region Methods (7) 

		// Public Methods (7) 

        public static Directory Append(Directory directory, string pathToAppend)
        {
            Directory dir = new Directory(directory.ToString());
            if (dir._path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                dir._path += pathToAppend.TrimStart(System.IO.Path.DirectorySeparatorChar);
            else if (dir._path.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                dir._path += pathToAppend.TrimStart(System.IO.Path.AltDirectorySeparatorChar);
            else
                dir._path += System.IO.Path.DirectorySeparatorChar.ToString() + pathToAppend;
            return dir;
        }

        public void Create()
        {
            if (!Exists())
                System.IO.Directory.CreateDirectory(_path);
        }

        public override void Delete()
        {
            System.IO.Directory.Delete(_path, true);
        }

        public override bool Exists()
        {
            return System.IO.Directory.Exists(_path);
        }

        public List<Directory> GetDirectories()
        {
            List<Directory> dirs = new List<Directory>();
            string[] paths = System.IO.Directory.GetDirectories(_path);

            for (int i = 0; i < paths.Length; i++)
                dirs.Add(new Directory(paths[i]));

            return dirs;
        }

        public string GetDirectoryShortName()
        {
            string name = _path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
            name = name.TrimEnd(System.IO.Path.AltDirectorySeparatorChar);

            return name.Substring(name.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
        }

        public List<File> GetFiles()
        {
            List<File> files = new List<File>();
            string[] paths = System.IO.Directory.GetFiles(_path);

            for (int i = 0; i < paths.Length; i++)
                files.Add(new File(paths[i]));

            return files;
        }

		#endregion Methods 
    }
}
