
namespace OpenDMS.Storage.Data
{
    public class ContentType
    {
        public string Name { get; private set; }

        public ContentType(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Determines the Content-Type of the given filepath based on the local system's registry
        /// </summary>
        /// <param name="filepath">The filename or filepath</param>
        /// <returns>A string containing the mime content type of the file</returns>
        public static ContentType GetFromRegistry(string filepath)
        {
            string name = "application/octetstream";
            string ext = System.IO.Path.GetExtension(filepath).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                name = rk.GetValue("Content Type").ToString();
            return new ContentType(name);
        }
    }
}
