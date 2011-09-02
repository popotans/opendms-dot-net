using System;
using System.Collections.Generic;
using OpenDMS.Storage.Data;

namespace OpenDMS.Storage.Providers
{
    public class CreateVersionArgs
    {
        public VersionId VersionId { get; set; }
        public string Revision { get; set; }
        public Metadata Metadata { get; set; }
        public Content Content { get; set; }

        public string Md5 { get; set; }
        public string Extension { get; set; }

        public CreateVersionArgs()
        {
        }
    }
}
