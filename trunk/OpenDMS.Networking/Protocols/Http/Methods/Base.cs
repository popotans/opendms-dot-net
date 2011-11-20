using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public abstract class Base
    {
        public static string METHOD { get { return "NULL"; } }
        public virtual string MethodName { get { return Base.METHOD; } }
    }
}
