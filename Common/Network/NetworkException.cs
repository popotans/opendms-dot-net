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

namespace Common.Network
{
    /// <summary>
    /// Represents errors that occur when transmitting network messages.
    /// </summary>
    public class NetworkException 
        : Exception
    {
        /// <summary>
        /// Represents the current <see cref="State"/> of the object that threw this exception.
        /// </summary>
        private State _state;
        /// <summary>
        /// Gets or sets the state of the object throwing this exception.
        /// </summary>
        /// <value>
        /// The state of the object.
        /// </value>
        public State WebState { get { return _state; } set { _state = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkException"/> class.
        /// </summary>
        /// <param name="state">The state of the throwing object.</param>
        public NetworkException(State state)
            : base()
        {
            _state = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkException"/> class.
        /// </summary>
        /// <param name="state">The state of the throwing object.</param>
        /// <param name="message">The message that describes the error.</param>
        public NetworkException(State state, string message)
            : base(message)
        {
            _state = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkException"/> class.
        /// </summary>
        /// <param name="state">The state of the throwing object.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception or null.</param>
        public NetworkException(State state, string message, Exception innerException)
            : base(message, innerException)
        {
            _state = state;
        }
    }
}
