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
using System.Web;

namespace OpenDMS
{
    public class ApiModule : IHttpModule
    {
        private ServicePointMap _map;
        private ServiceHandler _handler;

        public void Init(HttpApplication app)
        {
            if (_map == null)
            {
                _map = new ServicePointMap();
                _map.MapServicePoints(typeof(ServiceHandler));
            }

            if (_handler == null)
                _handler = new ServiceHandler();

            app.BeginRequest += new EventHandler(OnBeginRequest);
        }

        public void Dispose()
        {
            if(_handler != null)
                _handler.Dispose();
        }

        public void OnBeginRequest(Object s, EventArgs e)
        {
            HttpApplication app = (HttpApplication)s;
            ServicePointMapElement ele = _map.GetBestMatch(app.Request.Url.PathAndQuery, app.Request.HttpMethod);

            if (ele != null)
            {
                ele.MethodInfo.Invoke(_handler, new object[] { app } );
            }
        }
    }
}
