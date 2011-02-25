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

namespace HttpModule.Transactions
{
    /// <summary>
    /// Provides a mechanism for a lock on a transaction.
    /// </summary>
    public class Lock 
        : Common.NetworkPackage.DictionaryBase<string, object>
    {
        /// <summary>
        /// Gets or sets the username of the user that locked the transaction.
        /// </summary>
        /// <value>
        /// The user that locked the transaction.
        /// </value>
        public string LockedBy
        {
            get
            {
                if (ContainsKey("LockedBy"))
                    return (string)this["LockedBy"];
                else
                    return null;
            }
            set
            {
                if (ContainsKey("LockedBy"))
                    this["LockedBy"] = value;
                else
                    Add("LockedBy", value);
            }
        }

        /// <summary>
        /// Gets or sets the date/time that the lock was applied.
        /// </summary>
        /// <value>
        /// The date/time of locking.
        /// </value>
        public DateTime? Timestamp
        {
            get
            {
                if (ContainsKey("Timestamp"))
                    return (DateTime)this["Timestamp"];
                else
                    return null;
            }
            set
            {
                if (ContainsKey("Timestamp"))
                    this["Timestamp"] = value;
                else
                    Add("Timestamp", value);
            }
        }

        /// <summary>
        /// Gets or sets the date/time that the lock expires.
        /// </summary>
        /// <value>
        /// The date/time of expiration.
        /// </value>
        public DateTime? Expiry
        {
            get
            {
                if (ContainsKey("Expiry"))
                    return (DateTime)this["Expiry"];
                else
                    return null;
            }
            set
            {
                if (ContainsKey("Expiry"))
                    this["Expiry"] = value;
                else
                    Add("Expiry", value);
            }
        }

        /// <summary>
        /// Gets or sets the duration of the expiration
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public ulong? Duration
        {
            get
            {
                if (ContainsKey("Duration"))
                    return (ulong)this["Duration"];
                else
                    return null;
            }
            set
            {
                if (ContainsKey("Duration"))
                    this["Duration"] = value;
                else
                    Add("Duration", value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lock"/> class.
        /// </summary>
        public Lock()
            : base("Lock")
        {
        }

        public Lock(string lockedBy, DateTime timestamp, DateTime expiry, ulong duration) 
            : base("Lock")
        {
            LockedBy = lockedBy;
            Timestamp = timestamp;
            Expiry = expiry;
            Duration = duration;
        }
    }
}
