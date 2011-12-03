using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public class Post
        : Base
    {
        private const string METHOD = "POST";

        public Post()
            : base(METHOD)
        {
        }
    }
}
