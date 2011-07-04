using System.Diagnostics;
using System.Reflection;

namespace OpenDMS.IO
{
    public class FileStream : System.IO.FileStream
    {
		#region Fields (10) 

        private System.IO.FileAccess _access;
        private int _bufferSize;
        private System.DateTime _createdTimestamp;
        private object _creator;
        private File _file;
        private int _hashCode = 0;
        private StackFrame _lastAccess;
        private System.IO.FileMode _mode;
        private System.IO.FileOptions _options;
        private System.IO.FileShare _share;
        private bool _isClosed = false;

		#endregion Fields 

		#region Constructors (1) 

        public FileStream(File file, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileOptions options, int bufferSize, object creator)
            : base(file.ToString(), mode, access, share, bufferSize, options)
        {
            _file = file;
            _mode = mode;
            _access = access;
            _share = share;
            _options = options;
            _bufferSize = bufferSize;
            _creator = creator;
            _createdTimestamp = System.DateTime.Now;
            // Gets the calling method and stores it in _lastAccess - this is used to help resolve concurrent access request issues.
            _lastAccess = new StackTrace(true).GetFrame(1);
            FileSystem.Instance.RegisterHandle(this);
        }

		#endregion Constructors 

		#region Properties (17) 

        public System.IO.FileAccess Access 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _access;
            }
        }

        public int BufferSize 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _bufferSize;
            }
        }

        public override bool CanRead
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.CanWrite;
            }
        }

        public System.DateTime CreatedTimestamp 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _createdTimestamp;
            }
        }

        public object Creator 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _creator;
            }
        }

        public File File
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _file;
            }
        }

        public StackFrame LastAccess 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _lastAccess;
            }
        }

        public override long Length
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.Length;
            }
        }

        public System.IO.FileMode Mode 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _mode;
            }
        }

        public System.IO.FileOptions Options 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _options;
            }
        }

        public override long Position
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.Position;
            }
            set
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                base.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.ReadTimeout;
            }
            set
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                base.ReadTimeout = value;
            }
        }

        public System.IO.FileShare Share 
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return _share;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                _lastAccess = new StackTrace(true).GetFrame(1);
                return base.WriteTimeout;
            }
            set
            {
                base.WriteTimeout = value;
            }
        }

		#endregion Properties 

		#region Methods (20) 

		// Public Methods (19) 

        public override System.IAsyncResult BeginRead(byte[] array, int offset, int numBytes, System.AsyncCallback userCallback, object stateObject)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
        }

        public override System.IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, System.AsyncCallback userCallback, object stateObject)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
        }

        public override void Close()
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            if (!_isClosed)
            {
                _isClosed = true;
                base.Close();
            }
        }

        public void Copy(FileStream stream)
        {
            byte[] buffer = new byte[_bufferSize];
            int bytesRead;

            if (base.CanSeek)
                base.Position = 0;

            while ((bytesRead = base.Read(buffer, 0, buffer.Length)) > 0)
                stream.Write(buffer, 0, bytesRead);
        }

        public override int EndRead(System.IAsyncResult asyncResult)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.EndRead(asyncResult);
        }

        public override void EndWrite(System.IAsyncResult asyncResult)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.Flush();
        }

        public override void Flush(bool flushToDisk)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.Flush(flushToDisk);
        }

        public override int GetHashCode()
        {
            if (_hashCode != 0)
                return _hashCode;

            long ticks;
            
            ticks = CreatedTimestamp.Subtract(new System.DateTime(2011, 6, 18, 19, 12, 00, 0, System.DateTimeKind.Utc)).Ticks;
            if (ticks > int.MaxValue)
                throw new System.OverflowException("If this product is still in use July 6, 2079 it will begin throwing overflow exceptions.");

            _hashCode = (int)ticks;

            // Looks for a unique handle hashcode
            while (FileSystem.Instance.HandleExists(this))
                _hashCode--;

            return _hashCode;
        }

        public string GetLogString()
        {
            return "FullPath=" + File.ToString() + "\r\n" +
                    "Creator=" + Creator.GetType().FullName + "\r\n" +
                    "CreatedTimestamp=" + CreatedTimestamp.ToString("o") + "\r\n" +
                    "LastAccess=(File: " + _lastAccess.GetFileName() + ", Method: " + _lastAccess.GetMethod().ReflectedType.FullName + ", Line: " + _lastAccess.GetFileLineNumber().ToString() + ")\r\n" +
                    "Mode=" + Mode.ToString() + "\r\n" +
                    "Access=" + Access.ToString() + "\r\n" +
                    "Share=" + Share.ToString() + "\r\n" +
                    "Options=" + Options.ToString() + "\r\n" +
                    "BufferSize=" + BufferSize.ToString();
        }

        public override void Lock(long position, long length)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.Lock(position, length);
        }

        public override int Read(byte[] array, int offset, int count)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.Read(array, offset, count);
        }

        public override int ReadByte()
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.ReadByte();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.SetLength(value);
        }

        public override string ToString()
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            return base.ToString();
        }

        public override void Unlock(long position, long length)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.Unlock(position, length);
        }

        public override void Write(byte[] array, int offset, int count)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.Write(array, offset, count);
        }

        public override void WriteByte(byte value)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            base.WriteByte(value);
        }
		// Protected Methods (1) 

        protected override void Dispose(bool disposing)
        {
            _lastAccess = new StackTrace(true).GetFrame(1);
            FileSystem.Instance.CloseHandle(this);
            base.Dispose(disposing);
        }

		#endregion Methods 
    }
}
