using System;

namespace OpenDMS.Networking.Protocols.Http.Methods
{
    public abstract class Base
    {
        public virtual string MethodName { get; protected set; }

        public Base(string methodName)
        {
            MethodName = methodName;
        }
    }
}
