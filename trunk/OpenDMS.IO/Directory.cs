using System.Collections.Generic;

namespace OpenDMS.IO
{
    public class Directory : Location
    {
        public Directory(string path)
            : base(System.IO.Path.GetDirectoryName(path))
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

        public override void Delete()
        {
            System.IO.Directory.Delete(_path, true);
        }

        public List<Directory> GetDirectories()
        {
            List<Directory> dirs = new List<Directory>();
            string[] paths = System.IO.Directory.GetDirectories(_path);

            for (int i = 0; i < paths.Length; i++)
                dirs.Add(new Directory(paths[i]));

            return dirs;
        }

        public List<File> GetFiles()
        {
            List<File> files = new List<File>();
            string[] paths = System.IO.Directory.GetFiles(_path);

            for (int i = 0; i < paths.Length; i++)
                files.Add(new File(paths[i]));

            return files;
        }
    }
}
