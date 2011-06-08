using System;

namespace OpenDMS.Storage.Data
{
    public class ResourceId
    {
        public Guid Id { get; private set; }

        public ResourceId()
        {
            Id = Guid.Empty;
        }

        public ResourceId(Guid guid)
        {
            Id = guid;
        }

        public ResourceId(string guid)
        {
            Guid g;

            if (!Guid.TryParse(guid, out g))
                throw new ArgumentException("Argument must be able to be parsed to a System.Guid.");

            Id = g;
        }

        public static ResourceId Create()
        {
            return new ResourceId(Guid.NewGuid());
        }

        public override string ToString()
        {
            return Id.ToString("N");
        }
    }
}
