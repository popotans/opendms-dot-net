using System;
using System.IO;

namespace Common.FileSystem
{
    public class ResourceBase
    {
        private IO _fileSystem;
        private Logger _logger;
        private Guid _guid;
        private ResourceType _type;
        private string _extension;

        private bool _isOpen;
        private IOStream _stream;

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public string Extension
        {
            get { return _extension; }
            set { _extension = value; }
        }

        public string RelativeDirectory
        {
            get
            {
                switch (_type)
                {
                    case ResourceType.Meta:
                        return "meta" + Path.DirectorySeparatorChar.ToString();
                    case ResourceType.Data:
                        return "data" + Path.DirectorySeparatorChar.ToString();
                    default:
                        throw new IOException("Invalid ResourceType");
                }
            }
        }

        public string RelativeFilepath
        {
            get 
            {
                return RelativeDirectory + _guid.ToString("N") + _extension;
            }
        }

        public ulong FileLength
        {
            get
            {
                return _fileSystem.GetFileLength(RelativeFilepath);
            }
        }

        public ResourceBase(Guid guid, ResourceType type, string extension, IO fileSystem, Logger logger)
        {
            if(type == ResourceType.Unknown)
                throw new ArgumentException("Argument type must be Meta or Data.");

            _fileSystem = fileSystem;
            _logger = logger;
            _guid = guid;
            _type = type;
            _stream = null;
            _isOpen = false;

            if (type == ResourceType.Meta)
                _extension = ".xml";
            else
                _extension = extension;
        }

        public IOStream GetExclusiveReadStream(string openedLocation)
        {
            return GetStreamInternal(FileMode.Open, FileAccess.Read, FileShare.None,
                 FileOptions.None, "Common.FileSystem.FSObject.GetExclusiveReadStream() from " +
                 openedLocation);
        }

        public IOStream GetExclusiveWriteStream(string openedLocation)
        {
            return GetStreamInternal(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None,
                FileOptions.None, "Common.FileSystem.FSObject.GetExclusiveWriteStream() from " + 
                openedLocation);
        }

        public IOStream GetExclusiveWriteStreamUsingVersionScheme(UInt64 version,
            string openedLocation)
        {
            return GetStreamInternalUsingVersionScheme(version, FileMode.Open, FileAccess.Read, 
                FileShare.None, FileOptions.None, 
                "Common.FileSystem.FSObject.GetExclusiveWriteStream() from " +
                openedLocation);
        }

        public bool CopyCurrentToVersionScheme(UInt64 version, string openedLocation)
        {
            return _fileSystem.Copy(RelativeFilepath,
                RelativeDirectory + Guid.ToString("N") + "_" + version.ToString() + _extension);
        }

        public IOStream GetStream(FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            return GetStreamInternal(mode, access, share, options, 
                "Common.FileSystem.FSObject.GetStream() from " + openedLocation);
        }

        private IOStream GetStreamInternalUsingVersionScheme(UInt64 version, FileMode mode, 
            FileAccess access, FileShare share, FileOptions options, string openedLocation)
        {
            if (_stream != null)
                throw new IOException("The resource's stream is already open.");

            CreateContainingDirectory();

            try
            {
                _stream = _fileSystem.Open(RelativeDirectory + 
                    Guid.ToString("N") + "_" + version.ToString() + Extension, 
                    mode, access, share, options, openedLocation);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Failed to open the resource.\r\n" +
                        Logger.ExceptionToString(e));
            }

            if (_stream == null)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Failed to open the resource.\r\n");

                return _stream;
            }

            // The fact that it was opened will be logged through the FileSystem.IO object
            _isOpen = true;
            return _stream;
        }

        private IOStream GetStreamInternal(FileMode mode, FileAccess access, FileShare share,
            FileOptions options, string openedLocation)
        {
            if (_stream != null)
                throw new IOException("The resource's stream is already open.");

            CreateContainingDirectory();

            try
            {
                _stream = _fileSystem.Open(RelativeFilepath, mode, access, share,
                    options, openedLocation);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Failed to open the resource.\r\n" +
                        Logger.ExceptionToString(e));
            }

            if (_stream == null)
            {
                if (_logger != null)
                    _logger.Write(Logger.LevelEnum.Normal, "Failed to open the resource.\r\n");

                return _stream;
            }

            // The fact that it was opened will be logged through the FileSystem.IO object
            _isOpen = true;
            return _stream;
        }

        public void CloseStream()
        {
            if (_isOpen)
            {
                _fileSystem.Close(_stream);
                _stream = null;
                _isOpen = false;
            }
        }

        public void CreateContainingDirectory()
        {
            _fileSystem.CreateDirectoryPath(RelativeDirectory);
        }

        public bool ExistsOnFilesystem()
        {
            return _fileSystem.ResourceExists(RelativeFilepath);
        }

        public void DeleteFromFilesystem()
        {
            _fileSystem.Delete(RelativeFilepath);
        }

        public string ComputeMd5()
        {
            return _fileSystem.ComputeMd5(RelativeFilepath);
        }

        public bool VerifyMd5(string md5ToCompare)
        {
            return _fileSystem.VerifyMd5(RelativeFilepath, md5ToCompare);
        }
    }
}
