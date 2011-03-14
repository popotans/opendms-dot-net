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
using System.Web.Script.Serialization;

namespace Common.CouchDB
{
    /// <summary>
    /// The ConvertTo class is used to provide common conversion methods to the project
    /// </summary>
    internal class ConvertTo
    {
        /// <summary>
        /// Convert a Document to JSON
        /// </summary>
        /// <param name="doc">Document to Convert</param>
        /// <param name="dc">DocumentConverter to use</param>
        /// <returns>A JSON string</returns>
        internal static string DocumentToJson(Document doc, DocumentConverter dc)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { dc });
            return serializer.Serialize(doc);
        }

        /// <summary>
        /// Convert JSON to a Document
        /// </summary>
        /// <param name="json">JSON to Convert</param>
        /// <param name="dc">DocumentConverter to use</param>
        /// <returns>A Document</returns>
        internal static CouchDB.Document JsonToDocument(string json, DocumentConverter dc)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { dc });
            return serializer.Deserialize<Document>(json);
        }

        /// <summary>
        /// Convert JSON to a ServerResponse
        /// </summary>
        /// <param name="json">JSON to Convert</param>
        /// <returns>A ServerResponse</returns>
        internal static ServerResponse JsonToServerResponse(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new ServerResponseConverter() });
            return serializer.Deserialize<ServerResponse>(json);
        }

        /// <summary>
        /// Convert ASCII to UTF8
        /// </summary>
        /// <param name="ascii">ASCII to Convert</param>
        /// <returns>A UTF8 formatted string</returns>
        internal static string AsciiToUtf8(string ascii)
        {
            return System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(ascii));
        }
    }
}
