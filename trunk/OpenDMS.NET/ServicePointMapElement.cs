using System;
using System.Reflection;

namespace OpenDMS
{
    public class ServicePointMapElement
    {
        public MethodInfo MethodInfo { get; set; }
        public ServicePointAttribute ServicePoint { get; set; }

        public ServicePointMapElement(MethodInfo mi, ServicePointAttribute spa)
        {
            MethodInfo = mi;
            ServicePoint = spa;
        }
    }
}
