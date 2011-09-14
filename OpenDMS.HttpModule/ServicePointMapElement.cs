using System;
using System.Reflection;

namespace OpenDMS.HttpModule
{
    /// <summary>
    /// Represents an individual element of a <see cref="ServicePointMap"/>.
    /// </summary>
    public class ServicePointMapElement
    {
        /// <summary>
        /// Gets or sets the <see cref="MethodInfo"/>.
        /// </summary>
        /// <value>
        /// The <see cref="MethodInfo"/>.
        /// </value>
        public MethodInfo MethodInfo { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="ServicePointAttribute"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ServicePointAttribute"/>.
        /// </value>
        public ServicePointAttribute ServicePoint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicePointMapElement"/> class.
        /// </summary>
        /// <param name="mi">The <see cref="MethodInfo"/>.</param>
        /// <param name="spa">The <see cref="ServicePointAttribute"/>.</param>
        public ServicePointMapElement(MethodInfo mi, ServicePointAttribute spa)
        {
            MethodInfo = mi;
            ServicePoint = spa;
        }
    }
}
