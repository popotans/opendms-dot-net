using System;
using System.IO;

namespace Common.FileSystem
{
    public class FileState
    {
        /// <summary>
        /// The full path of the resource
        /// </summary>
        public string FullPath;
        /// <summary>
        /// The relative path of the resource
        /// </summary>
        public string RelativePath;
        /// <summary>
        /// The mode with which the resource was accessed
        /// </summary>
        public FileMode Mode;
        /// <summary>
        /// The access rights to the resource
        /// </summary>
        public FileAccess Access;
        /// <summary>
        /// The sharing rights of the resource with other requests
        /// </summary>
        public FileShare Share;
        /// <summary>
        /// The options associated with the resource
        /// </summary>
        public FileOptions Options;
        /// <summary>
        /// Signifies where in the code the resource was initially accessed
        /// </summary>
        public string Owner;
        /// <summary>
        /// The IOStream that has access to the resource
        /// </summary>
        public IOStream Stream;
        /// <summary>
        /// When the resource was accessed
        /// </summary>
        public DateTime OpenedAt;
        /// <summary>
        /// The size of the buffer
        /// </summary>
        public int BufferSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fullPath">The full path of the resource</param>
        /// <param name="relativePath">The relative path of the resource</param>
        /// <param name="mode">The mode with which the resource was accessed</param>
        /// <param name="access">The access rights to the resource</param>
        /// <param name="share">The sharing rights of the resource with other requests</param>
        /// <param name="options">The options associated with the resource</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="owner">Signifies where in the code the resource was initially accessed</param>
        /// <param name="iostream">The IOStream that has access to the resource</param>
        public FileState(string fullPath, string relativePath, FileMode mode, FileAccess access, 
            FileShare share, FileOptions options, int bufferSize, string owner, IOStream iostream)
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            Mode = mode;
            Access = access;
            Share = share;
            Options = options;
            Owner = owner;
            OpenedAt = DateTime.Now;
            Stream = iostream;
            BufferSize = bufferSize;
        }

        public string GetLogString()
        {
            return "FullPath=" + FullPath + "\r\n" +
                    "RelativePath=" + RelativePath + "\r\n" +
                    "Owner=" + Owner.ToString() + "\r\n" +
                    "OpenedAt=" + OpenedAt.ToString() + "\r\n" +
                    "Mode=" + Mode.ToString() + "\r\n" +
                    "Access=" + Access.ToString() + "\r\n" +
                    "Share=" + Share.ToString() + "\r\n" +
                    "Options=" + Options.ToString() + "\r\n" +
                    "BufferSize=" + BufferSize.ToString();
        }

        public bool DoesResourceConflict(FileState state)
        {
            if (state.RelativePath == RelativePath)
            {
                if ((state.Share & (FileShare)Access) != (FileShare)Access)
                {
                    return true;
                }
            }

            return false;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj.GetType() == typeof(FileState))
        //        return !DoesResourceConflict((FileState)obj);

        //    return false;
        //}
    }
}
