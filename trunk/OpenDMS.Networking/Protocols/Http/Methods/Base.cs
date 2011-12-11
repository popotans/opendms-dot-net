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

        public static Base Parse(string methodName)
        {
            switch (methodName)
            {
                case "DELETE":
                    return new Delete();
                case "GET":
                    return new Get();
                case "HEAD":
                    return new Head();
                case "POST":
                    return new Post();
                case "PUT":
                    return new Put();
                default:
                    throw new MethodParseException("Unknown method name.");
            }
        }
    }
}
