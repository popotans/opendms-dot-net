using System;
using System.Web;

namespace OpenDMS.HttpModule
{
    public class HttpModule : IHttpModule
    {
        /// <summary>
        /// A mapping of all service points.
        /// </summary>
        private ServicePointMap _map;
        /// <summary>
        /// The class handling the HTTP interface.
        /// </summary>
        private ServiceHandler _handler;
        
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
            if (_handler != null)
                _handler.Dispose();
        }

        /// <summary>
        /// Initializes this HttpModule
        /// </summary>
        /// <param name="app">The <see cref="HttpApplication"/>.</param>
        public void Init(HttpApplication app)
        {
            if (_map == null)
            {
                _map = new ServicePointMap();
                _map.MapServicePoints(typeof(ServiceHandler));
            }

            if (_handler == null)
            {
                _handler = new ServiceHandler();
                _handler.Init();
            }

            app.BeginRequest += new EventHandler(OnBeginRequest);
        }

        /// <summary>
        /// Called when a HTTP request begins.
        /// </summary>
        /// <param name="s">The sender (<see cref="HttpApplication"/>).</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void OnBeginRequest(Object s, EventArgs e)
        {
            HttpApplication app = (HttpApplication)s;
            ServicePointMapElement ele = _map.GetBestMatch(app.Request.Url.PathAndQuery, app.Request.HttpMethod);

            if (ele != null)
            {
                ele.MethodInfo.Invoke(_handler, new object[] { app });
            }
        }
    }
}
