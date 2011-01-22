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
