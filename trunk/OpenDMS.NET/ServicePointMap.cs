/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Reflection;
using System.Collections.Generic;

namespace HttpModule
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
