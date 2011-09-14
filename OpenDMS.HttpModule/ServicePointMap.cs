using System;
using System.Reflection;
using System.Collections.Generic;

namespace OpenDMS.HttpModule
{
    /// <summary>
    /// Provides a device to map all <see cref="ServicePointAttribute"/> to methods.
    /// </summary>
    public class ServicePointMap
    {
        /// <summary>
        /// A collection of <see cref="ServicePointMapElement"/>.
        /// </summary>
        private List<ServicePointMapElement> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePointMap"/> class.
        /// </summary>
        public ServicePointMap()
        {
            _map = new List<ServicePointMapElement>();
        }

        /// <summary>
        /// Maps the service points.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the class to map.</param>
        public void MapServicePoints(Type type)
        {
            MethodInfo[] infos = type.GetMethods();
            ServicePointAttribute spa;
            object[] attributes;

            for (int i = 0; i < infos.Length; i++)
            {
                attributes = infos[i].GetCustomAttributes(true);
                for (int j = 0; j < attributes.Length; j++)
                {
                    if (attributes[j].GetType() == typeof(ServicePointAttribute))
                    {
                        spa = (ServicePointAttribute)attributes[j];
                        _map.Add(new ServicePointMapElement(infos[i], spa));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the best match for the path and verb.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <param name="verb">The HTTP verb.</param>
        /// <returns>A ServicePointMapElement for the arguments.</returns>
        public ServicePointMapElement GetBestMatch(string virtualPath, string verb)
        {
            ServicePointMapElement ele = null;
            int matchVal, eleVal = -1;

            for (int i = 0; i < _map.Count; i++)
            {
                matchVal = _map[i].ServicePoint.GetMatchRate(virtualPath, verb);
                if (matchVal > 0)
                {
                    if (ele == null)
                    {
                        ele = _map[i];
                        eleVal = matchVal;
                    }
                    else
                    {
                        if (eleVal < matchVal)
                        {
                            ele = _map[i];
                            eleVal = matchVal;
                        }
                        else if (eleVal == matchVal)
                        {
                            if (ele.ServicePoint.Verb == _map[i].ServicePoint.Verb)
                                throw new Exception("Identical path match is invalid");

                            if (ele.ServicePoint.Verb == ServicePointAttribute.VerbType.ALL)
                            {
                                // Specific takes priority - new wins
                                ele = _map[i];
                                eleVal = matchVal;
                            }
                            else if (_map[i].ServicePoint.Verb == ServicePointAttribute.VerbType.ALL)
                            {
                                // Specific takes priority - old wins ~ do nothing
                            }
                            else
                                throw new Exception("Identical path match is invalid");
                        }
                    }
                }
            }

            return ele;
        }
    }
}
