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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;

namespace Common.CouchDB
{
    /// <summary>
    /// The ServerResponse class represents a response from a CouchDB server
    /// </summary>
    public class ServerResponse
    {
        /// <summary>
        /// The _id of the response
        /// </summary>
        private string _id;
        /// <summary>
        /// The _rev of the response
        /// </summary>
        private string _rev;
        /// <summary>
        /// The "ok" field of the response
        /// </summary>
        private bool? _ok;
        /// <summary>
        /// The "error" field of the response
        /// </summary>
        private string _error;
        /// <summary>
        /// The "reason" field of the response
        /// </summary>
        private string _reason;
        /// <summary>
        /// A reference to any underlying exception
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Gets the _id
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the _rev
        /// </summary>
        public string Rev
        {
            get { return _rev; }
        }

        /// <summary>
        /// Gets the ok - true if positive, false is negative, null if not set
        /// </summary>
        public bool? Ok
        {
            get { return _ok; }
        }

        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Gets the reason supplied by the server
        /// </summary>
        public string Reason
        {
            get { return _reason; }
        }

        /// <summary>
        /// Gets the Exception
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Constructer - sets everything to null
        /// </summary>
        public ServerResponse()
        {
            _id = null;
            _rev = null;
            _error = null;
            _ok = null;
            _reason = null;
            _exception = null;
        }

        /// <summary>
        /// Constructor - sets just the ok
        /// </summary>
        /// <param name="ok">Value for ok</param>
        public ServerResponse(bool? ok)
        {
            _id = null;
            _rev = null;
            _error = null;
            _ok = ok;
            _reason = null;
            _exception = null;
        }

        /// <summary>
        /// Constructor - sets the 3 argument values
        /// </summary>
        /// <param name="ok">Value for ok</param>
        /// <param name="id">The _id</param>
        /// <param name="rev">The _rev</param>
        public ServerResponse(bool? ok, string id, string rev)
        {
            _ok = ok;
            _id = id;
            _rev = rev;
            _error = null;
            _reason = null;
            _exception = null;
        }

        /// <summary>
        /// Constructor - sets the 2 argument values
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="reason">The server reason</param>
        public ServerResponse(string error, string reason)
        {
            _ok = false;
            _id = null;
            _rev = null;
            _error = error;
            _reason = reason;
            _exception = null;
        }

        /// <summary>
        /// Constructor - sets the 2 argument values
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="reason">The server reason</param>
        /// <param name="exception">The exception thrown</param>
        public ServerResponse(string error, string reason, Exception exception)
        {
            _ok = false;
            _id = null;
            _rev = null;
            _error = error;
            _reason = reason;
            _exception = exception;
        }

        /// <summary>
        /// Instantiates a new instance of ServerResponse
        /// </summary>
        /// <param name="dictionary">The dictionary from which values are extracted</param>
        /// <returns></returns>
        public static ServerResponse Instantiate(Dictionary<string, object> dictionary)
        {
            ServerResponse response;
            string e,r;

            if (dictionary.ContainsKey("ok") && (bool)dictionary["ok"])
            { // Success
                if (dictionary.ContainsKey("id") && !string.IsNullOrEmpty((string)dictionary["id"]) &&
                    dictionary.ContainsKey("rev") && !string.IsNullOrEmpty((string)dictionary["rev"]))
                    response = new ServerResponse(true, (string)dictionary["id"], (string)dictionary["rev"]);
                else if (dictionary.ContainsKey("rev") && !string.IsNullOrEmpty((string)dictionary["rev"]))
                    response = new ServerResponse(true, null, (string)dictionary["rev"]);
                else
                    response = new ServerResponse(true);
            }
            else if (dictionary.ContainsKey("id") && dictionary.ContainsKey("rev"))
            { // Delete uses this
                if (!string.IsNullOrEmpty((string)dictionary["id"]) &&
                    !string.IsNullOrEmpty((string)dictionary["rev"]))
                    response = new ServerResponse(true, (string)dictionary["id"], (string)dictionary["rev"]);
                else
                    throw new FormatException("The dictionary must contain a string value for id and rev");
            }
            else
            { // Error
                e = r = null;

                if (dictionary.ContainsKey("error") && string.IsNullOrEmpty((string)dictionary["error"]))
                    e = (string)dictionary["error"];
                else
                    throw new FormatException("The dictionary must contain a string value for error");

                if (dictionary.ContainsKey("reason") && string.IsNullOrEmpty((string)dictionary["reason"]))
                    r = (string)dictionary["reason"];
                else
                    throw new FormatException("The dictionary must contain a string value for reason");

                response = new ServerResponse(e, r);
            }

            return response;
        }
    }

    internal class ServerResponseConverter : JavaScriptConverter
    {
        /// <summary>
        /// Deserializes a dictionary to a ServerResponse
        /// </summary>
        /// <param name="dictionary">The dictionary to use</param>
        /// <param name="type">The type of object</param>
        /// <param name="serializer">A reference to the JavaScriptSerializer</param>
        /// <returns>A ServerResponse</returns>
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type != typeof(ServerResponse))
                return null;

            return ServerResponse.Instantiate((Dictionary<string, object>)dictionary);
        }

        /// <summary>
        /// CANNOT BE USED
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new InvalidOperationException("Cannot serialize ServerResponse");
        }

        /// <summary>
        /// The types supported by this JavaScriptConverter
        /// </summary>
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(ServerResponse) })); }
        }
    }
}
