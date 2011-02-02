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
    /// A reference marker indicating the version of the asset as a whole.
    /// </summary>
    public class ETag
    {
        /// <summary>
        /// A string value representing the version.
        /// </summary>
        private string _value;
        /// <summary>
        /// Gets a string value representing the version.
        /// </summary>
        public string Value { get { return _value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETag"/> class.
        /// </summary>
        /// <param name="value">A string value representing the version.</param>
        public ETag(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ETag"/> is newer than this instance.
        /// </summary>
        /// <param name="compare">The <see cref="ETag"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified compare is newer; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNewer(ETag compare)
        {
            UInt64 a, b;

            if (compare == null)
                return true;

            try
            {
                a = UInt64.Parse(_value);
                b = UInt64.Parse(compare.Value);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Value must be castable to UInt64.");
            }

            return a > b;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ETag"/> is older than this instance.
        /// </summary>
        /// <param name="compare">The <see cref="ETag"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified compare is older; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOlder(ETag compare)
        {
            UInt64 a, b;

            if (compare == null)
                return false;

            try
            {
                a = UInt64.Parse(_value);
                b = UInt64.Parse(compare.Value);
            }
            catch (Exception)
            {
                throw new InvalidCastException("Value must be castable to UInt64.");
            }

            return a < b;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Obj cannot be null");

            if (obj.GetType() == typeof(ETag))
                return _value == ((ETag)obj).Value;

            throw new Exception("Invalid argument type");
        }

        /// <summary>
        /// Increments this instance.
        /// </summary>
        /// <returns>The incremented <see cref="ETag"/>.</returns>
        public ETag Increment()
        {
            UInt64 value = UInt64.Parse(_value);
            value++;
            _value = value.ToString();
            return this;
        }

        /// <summary>
        /// Adds the specified value to this instance.
        /// </summary>
        /// <param name="value">An integer by which to increment this instance.</param>
        /// <returns>The incremented <see cref="ETag"/>.</returns>
        public ETag Increment(int value)
        {
            UInt64 temp = UInt64.Parse(_value);
            temp = temp + (UInt64)value;
            _value = temp.ToString();
            return this;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="etag">The <see cref="ETag"/>.</param>
        /// <param name="value">An integer value by which to increment the <see cref="ETag"/>.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static ETag operator +(ETag etag, int value)
        {
            UInt64 val = UInt64.Parse(etag.Value) + (UInt64)value;
            return new ETag(val.ToString());
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _value;
        }

        /// <summary>
        /// Exports to network representation.
        /// </summary>
        /// <returns>A <see cref="NetworkPackage.ETag"/> representing this <see cref="ETag"/>.</returns>
        public NetworkPackage.ETag ExportToNetworkRepresentation()
        {
            if (string.IsNullOrEmpty(Value))
                throw new Exception("Value cannot be empty");

            NetworkPackage.ETag etag = new NetworkPackage.ETag();

            etag.Add("Value", this.Value);

            return etag;
        }
    }
}
