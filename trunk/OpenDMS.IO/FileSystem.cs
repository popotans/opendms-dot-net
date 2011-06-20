using System.Collections.Generic;

namespace OpenDMS.IO
{
    public sealed class FileSystem : Singleton<FileSystem>
    {
        private List<FileStream> _handles = null; // Cannot use Dictionary because we can have multiple access to a single file

        public Directory Root { get; private set; }
        public int BufferSize { get; private set; }

        public void Initialize(Directory root, int bufferSize)
        {
            Root = root;
            BufferSize = bufferSize;
            _isInitialized = true;
        }

        public FileSystem()
        {
            _handles = new List<FileStream>();
        }

        public void RegisterHandle(FileStream fs)
        {
            if (!_isInitialized)
                throw new NotInitializedException("Must call Initialize.");

            List<FileStream>.Enumerator en;

            lock (_handles)
            {
                en = _handles.GetEnumerator();

                while (en.MoveNext())
                {
                    if (fs.File.ToString() == en.Current.File.ToString())
                    {
                        if ((en.Current.Share & (System.IO.FileShare)fs.Access) != (System.IO.FileShare)fs.Access)
                        {
                            if (Logger.FileSystem != null)
                                Logger.FileSystem.Error("The resource is already open and the requested access conflicts with the existing share.\r\n" + en.Current.GetLogString());
                            throw new AccessException("Resource in use.");
                        }
                    }
                }

                if (_handles.Contains(fs))
                    throw new HandleExistsException("A handle already exists.");

                _handles.Add(fs);

                if (Logger.FileSystem != null)
                    Logger.FileSystem.Debug("Handle registered for " + fs.GetLogString());
            }
        }

        public void CloseHandle(FileStream fs)
        {
            if (!_isInitialized)
                throw new NotInitializedException("Must call Initialize.");

            lock (_handles)
            {
                if (!_handles.Contains(fs))
                    throw new HandleExistsException("The handle does not exist.");
                
                // We force close and dispose, we will not tolerate unmanaged IO
                fs.Close();
                fs.Dispose();

                // Drop it from handles - btw we can say remove knowing it will hit the correct resource
                // because it is located using the HashCode, which will have customized to always test itself for
                // uniqueness within handles.
                _handles.Remove(fs);

                if (Logger.FileSystem != null)
                    Logger.FileSystem.Debug("Handle released for " + fs.GetLogString());
            }
        }

        public bool HandleExists(FileStream fs)
        {
            if (!_isInitialized)
                throw new NotInitializedException("Must call Initialize.");

            lock (_handles)
            {
                return _handles.Contains(fs);
            }
        }
    }
}
