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

namespace Common.Data
{
    /// <summary>
    /// Represents errors that occur when an Asset's state is not proper for the requested action.
    /// </summary>
    public class InvalidAssetStateException 
        : Exception
    {
        /// <summary>
        /// Represents the current <see cref="AssetState"/> of the object that threw this exception.
        /// </summary>
        public AssetState State;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAssetStateException"/> class.
        /// </summary>
        /// <param name="state">The current <see cref="AssetState"/> of the object that threw this exception.</param>
        public InvalidAssetStateException(AssetState state)
            : base()
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAssetStateException"/> class.
        /// </summary>
        /// <param name="state">The current <see cref="AssetState"/> of the object that threw this exception.</param>
        /// <param name="message">The message that describes the error.</param>
        public InvalidAssetStateException(AssetState state, string message)
            : base(message)
        {
            State = state;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidAssetStateException"/> class.
        /// </summary>
        /// <param name="state">The current <see cref="AssetState"/> of the object that threw this exception.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception or null.</param>
        public InvalidAssetStateException(AssetState state, string message, Exception innerException)
            : base(message, innerException)
        {
            State = state;
        }
    }
}
