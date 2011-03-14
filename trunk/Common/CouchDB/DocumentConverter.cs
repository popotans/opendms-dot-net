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
    internal abstract class DocumentConverter : JavaScriptConverter
    {
        /// <summary>
        /// Types supported by this converter
        /// </summary>
        public override System.Collections.Generic.IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(Document) })); }
        }

        /// <summary>
        /// Converter for the HTTP PUT command
        /// </summary>
        internal class Put : DocumentConverter
        {
            /// <summary>
            /// Serializes the argument object to JSON.
            /// </summary>
            /// <param name="obj">Object to serialize</param>
            /// <param name="serializer">Reference to the JavaScriptSerializer</param>
            /// <returns>An IDictionary of strings and objects</returns>
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                if (obj == null)
                    throw new ArgumentNullException("obj", "The argument obj cannot be a null value");
                if (obj.GetType() != typeof(CouchDB.Document))
                    throw new ArgumentException("obj must be of type CouchDB.Document", "obj");

                Document doc;
                KeyValuePair<string, object> kvp;
                Dictionary<string, object>.Enumerator e;
                Dictionary<string, object> dir = new Dictionary<string, object>();

                try
                {
                    doc = (Document)obj;
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException("The argument obj cannot be cast to type CouchDB.Document", ex);
                }

                if (doc == null)
                    throw new ArgumentNullException("obj", "The argument obj casts to a null value which cannot be handled");
                

                e = doc.GetPropertyEnumerator();

                // Add the base key/value pairs
                dir.Add("_id", doc.Id);

                if(!string.IsNullOrEmpty(doc.Rev))
                    dir.Add("_rev", doc.Rev);

                while (e.MoveNext())
                {
                    kvp = (KeyValuePair<string, object>)e.Current;
                    dir.Add(kvp.Key, kvp.Value);
                }

                if (doc.Attachments.Count > 0)
                {
                    kvp = doc.Attachments.Serialize();
                    dir.Add(kvp.Key, kvp.Value);
                }

                return dir;
            }

            /// <summary>
            /// Cannot be called
            /// </summary>
            /// <param name="dictionary"></param>
            /// <param name="type"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object Deserialize(System.Collections.Generic.IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new InvalidOperationException("Cannot deserialize PUT");
            }
        }

        internal class Post : DocumentConverter
        {
            /// <summary>
            /// Serializes the argument object to JSON.
            /// </summary>
            /// <param name="obj">Object to serialize</param>
            /// <param name="serializer">Reference to the JavaScriptSerializer</param>
            /// <returns>An IDictionary of strings and objects</returns>
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                if (obj == null)
                    throw new ArgumentNullException("obj", "The argument obj cannot be a null value");
                if (obj.GetType() != typeof(CouchDB.Document))
                    throw new ArgumentException("obj must be of type CouchDB.Document", "obj");

                Document doc;
                KeyValuePair<string, object> kvp;
                Dictionary<string, object>.Enumerator e;
                Dictionary<string, object> dir = new Dictionary<string, object>();


                try
                {
                    doc = (Document)obj;
                }
                catch (Exception ex)
                {
                    throw new ArgumentNullException("The argument obj cannot be cast to type CouchDB.Document", ex);
                }

                if (doc == null)
                    throw new ArgumentNullException("obj", "The argument obj casts to a null value which cannot be handled");


                e = doc.GetPropertyEnumerator();

                // Leave _id and _rev out for Posts
                //dir.Add("_id", doc.Id);
                //dir.Add("_rev", doc.Rev);

                while (e.MoveNext())
                {
                    kvp = (KeyValuePair<string, object>)e.Current;
                    dir.Add(kvp.Key, kvp.Value);
                }

                return dir;
            }

            /// <summary>
            /// Cannot be called
            /// </summary>
            /// <param name="dictionary"></param>
            /// <param name="type"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object Deserialize(System.Collections.Generic.IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new InvalidOperationException("Cannot deserialize Post");
            }
        }

        internal class Get : DocumentConverter
        {
            /// <summary>
            /// Cannot be called
            /// </summary>
            /// <param name="obj">Object to serialize</param>
            /// <param name="serializer">Reference to the JavaScriptSerializer</param>
            /// <returns>An IDictionary of strings and objects</returns>
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                throw new InvalidOperationException("Cannot serialize Get");
            }

            /// <summary>
            /// Cannot be called
            /// </summary>
            /// <param name="dictionary">The dictionary to deserialize</param>
            /// <param name="type">The type of object to be instantiated</param>
            /// <param name="serializer">A reference to the JavaScriptConverter</param>
            /// <returns></returns>
            public override object Deserialize(System.Collections.Generic.IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");

                if (type != typeof(CouchDB.Document))
                    throw new ArgumentException("type must be a CouchDB.Document", "type");

                return Document.Instantiate((Dictionary<string, object>)dictionary);
            }
        }
    }
}
