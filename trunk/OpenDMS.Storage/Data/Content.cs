
namespace OpenDMS.Storage.Data
{
    public class Content
    {
        public long Length { get; private set; }
        public ContentType ContentType { get; private set; }
        public string LocalFilepath { get; private set; }
        public System.IO.Stream Stream { get; private set; }

        public Content()
        {
        }

        public Content(long length, string localFilepath)
            : this(length, ContentType.GetFromRegistry(localFilepath), localFilepath)
        {
        }

        public Content(long length, ContentType contentType, string localFilepath)
        {
            Length = length;
            ContentType = contentType;
            LocalFilepath = localFilepath;
        }

        public Content(long length, ContentType contentType, System.IO.Stream stream)
        {
            Length = length;
            ContentType = contentType;
            Stream = stream;
        }
    }
}
