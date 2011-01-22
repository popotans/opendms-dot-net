using System;

namespace WindowsClient
{
    public class Utilities
    {
        public static string MakeBytesHumanReadable(long bytes)
        {
            string size;

            if (bytes > 1073741824)
                size = string.Format("{0:#.##} GB", ((double)bytes / 1073741824D));
            else if (bytes > 1048576)
                size = string.Format("{0:#.##} MB", ((double)bytes / 1048576D));
            else if (bytes > 1024)
                size = string.Format("{0:#.##} KB", ((double)bytes / 1024D));
            else
                size = string.Format("{0:#} Bytes", bytes);

            return size;
        }

        public static string MakeBytesHumanReadable(ulong bytes)
        {
            string size;

            if (bytes > 1073741824)
                size = string.Format("{0:#.##} GB", ((double)bytes / 1073741824D));
            else if (bytes > 1048576)
                size = string.Format("{0:#.##} MB", ((double)bytes / 1048576D));
            else if (bytes > 1024)
                size = string.Format("{0:#.##} KB", ((double)bytes / 1024D));
            else
                size = string.Format("{0:#} Bytes", bytes);

            return size;
        }

        public static string GetAppPath()
        {
            string str = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (!str.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) 
                str += System.IO.Path.DirectorySeparatorChar.ToString();
            return str;
        }
    }
}
