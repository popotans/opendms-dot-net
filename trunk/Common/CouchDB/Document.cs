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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Common.CouchDB
{
    /// <summary>
    /// The Document class is used to represent a CouchDB Document
    /// </summary>
    public class Document
    {
        private const int RESET = 0x00;
        private const int CAN_DOWNLOAD = 0x01;
        private const int CAN_COPY = CAN_DOWNLOAD;
        private const int CAN_UPDATE = 0x02;
        private const int CAN_DELETE = 0x04;
        private const int CAN_CREATE = 0x08;
        private const int CAN_ALL = CAN_DOWNLOAD | CAN_UPDATE | CAN_DELETE | CAN_CREATE;

        /// <summary>
        /// The state of the instance, can be any combination of: RESET, CAN_DOWNLOAD, CAN_COPY, CAN_UPLOAD, CAN_DELETE, CAN_CREATE, CAN_ALL
        /// </summary>
        private int _state;

        /// <summary>
        /// The _id of the CouchDB Document
        /// </summary>
        private string _id;

        /// <summary>
        /// The _rev of the CouchDB Document
        /// </summary>
        private string _rev;

        /// <summary>
        /// An AttachmentCollection representing all the attachments housed within the Document
        /// </summary>
        private AttachmentCollection _attachments;

        /// <summary>
        /// User properties, any property that is not _id, _rev or _attachments is stored within here
        /// </summary>
        private Dictionary<string, object> _properties;

        /// <summary>
        /// Where this document should be copied to on the remote CouchDB server (only used when the Copy method on this class is used)
        /// </summary>
        private Document _destinationForCopy;

        /// <summary>
        /// Handles general events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        public delegate void EventHandler(Document sender, Http.Client httpClient);
        /// <summary>
        /// Handles progress events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="httpClient">The <see cref="Http.Client"/> handling communications.</param>
        /// <param name="httpConnection">The <see cref="Http.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet.</param>
        /// <param name="headersTotal">The total size headers in bytes.</param>
        /// <param name="contentTotal">The total size of content in bytes.</param>
        /// <param name="total">The total size of all data in bytes.</param>
        public delegate void ProgressEventHandler(Document sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        
        /// <summary>
        /// Fired when a timeout occurs - *NOTE* a timeout will prevent OnComplete from firing.
        /// </summary>
        public event EventHandler OnTimeout;

        /// <summary>
        /// Fired to indicate progress of an upload
        /// </summary>
        public event ProgressEventHandler OnUploadProgress;

        /// <summary>
        /// Fired to indicate progress of a download
        /// </summary>
        public event ProgressEventHandler OnDownloadProgress;

        #region Properties

        /// <summary>
        /// Gets the _id of the CouchDB Document
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets/Sets the _rev of the CouchDB Document
        /// </summary>
        public string Rev
        {
            get { return _rev; }
            set 
            { 
                _rev = value;
                ResetState();
            }
        }

        /// <summary>
        /// Gets an AttachmentCollection that represents the Attachments housed by the Document
        /// </summary>
        public AttachmentCollection Attachments
        {
            get { return _attachments; }
        }

        /// <summary>
        /// Gets the _state
        /// </summary>
        public int State
        {
            get { return _state; }
        }

        #endregion

        #region Constructors and Instantiation

        /// <summary>
        /// Null Constructor
        /// </summary>
        public Document()
        {
            _id = null;
            _rev = null;
            _attachments = new AttachmentCollection();
            _properties = new Dictionary<string, object>();
            ResetState();

            Logger.General.Debug("Common.CouchDB.Document instantiated.");
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="doc">The Document to copy</param>
        public Document(Document doc)
        {
            Dictionary<string, object>.Enumerator dictEnum;
            _state = doc.State;
            _id = doc.Id;
            _rev = doc.Rev;

            for(int i=0; i<doc.Attachments.Count; i++)
            {
                _attachments.Add(doc.Attachments[i]);
            }

            dictEnum = new Dictionary<string, object>.Enumerator();

            while (dictEnum.MoveNext())
            {
                _properties.Add((string)dictEnum.Current.Key, dictEnum.Current.Value);
            }

            Logger.General.Debug("Common.CouchDB.Document instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The _id of the CouchDB Document</param>
        public Document(string id)
        {
            _id = id;
            _rev = null;
            _attachments = new AttachmentCollection();
            _properties = new Dictionary<string, object>();
            ResetState();

            Logger.General.Debug("Common.CouchDB.Document instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The _id of the Document</param>
        /// <param name="rev">The _rev of the Document</param>
        public Document(string id, string rev)
        {
            _id = id;
            _rev = rev;
            _attachments = new AttachmentCollection();
            _properties = new Dictionary<string, object>();
            ResetState();

            Logger.General.Debug("Common.CouchDB.Document instantiated.");
        }

        /// <summary>
        /// Creates a new instance of a CouchDB.Document
        /// </summary>
        /// <param name="dictionary">Dictionary to iterate for property values</param>
        /// <returns>A new CouchDB.Document instance</returns>
        public static Document Instantiate(Dictionary<string, object> dictionary)
        {
            // Make sure the keys exist
            if (!dictionary.ContainsKey("_id") || !dictionary.ContainsKey("_rev"))
            {
                throw new ArgumentException("dictionary must contain valid keys for: _id and _rev", "dictionary");
            }

            // Make sure their values exist and are strings
            try
            {
                if (string.IsNullOrEmpty((string)dictionary["_id"]) ||
                    string.IsNullOrEmpty((string)dictionary["_rev"]))
                {
                    throw new ArgumentException("dictionary must contain non-null and non-empty keys for: _id and _rev.", "dictionary");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("dictionary must contain string values for keys: _id and _rev.", "dictionary", e);
            }

            Document doc;
            Dictionary<string, object>.Enumerator de;
            KeyValuePair<string, object> kvp;

            // Create the new Document instance
            doc = new Document((string)dictionary["_id"],
                (string)dictionary["_rev"]);

            // Assign it any remaining properties from the dictionary
            de = dictionary.GetEnumerator();
            while (de.MoveNext())
            {
                kvp = de.Current;
                if (kvp.Key != "_id" && kvp.Key != "_rev" && kvp.Key != "_attachments")
                { // Make sure to remove the primary keys
                    doc.AddProperty(kvp.Key, kvp.Value);
                }
            }

            if (dictionary.ContainsKey("_attachments"))
            {
                doc.ExtractAttachments(dictionary["_attachments"]);
            }

            return doc;
        }

        /// <summary>
        /// Extracts the Attachments from a Dictionary's value property
        /// </summary>
        /// <param name="value">The Dictionary's value property from which the Attachments can be extracted</param>
        /// <returns>An AttachmentCollection representing the contained Attachments</returns>
        private AttachmentCollection ExtractAttachments(object value)
        {
            if (value.GetType() != typeof(Dictionary<string, object>))
                throw new ArgumentException("value must be of type Dictionary<string, object>", "value");

            Dictionary<string, object> dict = (Dictionary<string, object>)value;
            Dictionary<string, object>.Enumerator ienum = dict.GetEnumerator();
            Attachment att;

            while (ienum.MoveNext())
            {
                att = Attachment.Instantiate(this, ienum.Current);
                _attachments.Add(att);
            }

            return _attachments;
        }

        #endregion

        #region AddProperty

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void AddProperty(string name, bool value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, byte value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, char value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, decimal value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, double value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, float value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, DateTime value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, DateTimeOffset value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, int value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, long value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, object value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, sbyte value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, short value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, string value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, uint value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, ulong value)
        {
            _properties.Add(name, value);
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(string name, ushort value)
        {
            _properties.Add(name, value);
        }

        #endregion

        /// <summary>
        /// Removes a property from the Document
        /// </summary>
        /// <param name="name"></param>
        public void RemoveProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.", "name");

            _properties.Remove(name);
        }

        /// <summary>
        /// Gets or sets the value of a property
        /// </summary>
        /// <param name="name">The property name</param>
        /// <returns>An object of the value corresponding to the key of the property name</returns>
        public object this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Name cannot be null or empty.", "name");

                if (_properties.ContainsKey(name))
                    return _properties[name];
                else
                    return null;
            }
            set
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Name cannot be null or empty.", "name");

                if (_properties.ContainsKey(name))
                    _properties[name] = value;
                else
                    _properties.Add(name, value);
            }
        }

        /// <summary>
        /// Checks to see if a key exists in the properties
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.", "name");

            return _properties.ContainsKey(name);
        }

        /// <summary>
        /// Resets the _state property accordingly to the properties currently set within the instance
        /// </summary>
        private void ResetState()
        {
            _state = RESET;

            // Create = non null _properties and _attachments
            // Download = Create and _id
            // Copy = Download
            // Upload = Download and _rev
            // Delete = Upload

            if (_properties == null)
                throw new CouchDBException("The property _properties cannot be null, perhaps you need to call a constructor?");
            if (_attachments == null)
                throw new CouchDBException("The property _attachments cannot be null, perhaps you need to call a constructor?");

            _state = _state | CAN_CREATE;

            if (!string.IsNullOrEmpty(_id))
            {
                _state = _state | CAN_DOWNLOAD | CAN_COPY;

                if (!string.IsNullOrEmpty(_rev))
                    _state = CAN_ALL;
            }
        }

        /// <summary>
        /// True if the Document can be downloaded
        /// </summary>
        public bool CanDownload
        {
            get { return CheckState(CAN_DOWNLOAD); }
        }

        /// <summary>
        /// True if the Document can be copied
        /// </summary>
        public bool CanCopy
        {
            get { return CheckState(CAN_COPY); }
        }

        /// <summary>
        /// True if the Document can be updated
        /// </summary>
        public bool CanUpdate
        {
            get { return CheckState(CAN_UPDATE); }
        }

        /// <summary>
        /// True if Document can be deleted
        /// </summary>
        public bool CanDelete
        {
            get { return CheckState(CAN_DELETE); }
        }

        /// <summary>
        /// True if the Document can be created
        /// </summary>
        public bool CanCreate
        {
            get { return CheckState(CAN_CREATE); }
        }

        /// <summary>
        /// Checks to see if the current _state of the Document supports the requested action
        /// </summary>
        /// <param name="requiredState">The action being requested</param>
        /// <returns>True if supported, false if not supported</returns>
        private bool CheckState(int requiredState)
        {
            if ((_state & requiredState) == requiredState)
                return true;
            return false;
        }

        /// <summary>
        /// Synchronously downloads a Document from the Server to the local system
        /// </summary>
        /// <param name="db">The Database housing the Document</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Download(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            // Check the _state
            if (!CheckState(CAN_DOWNLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);


            Http.Client httpClient;
            Http.Methods.HttpGet httpGet;
            Http.Methods.HttpResponse httpResponse = null;
            string json;
            List<string> stringList;
            ArrayList al;
            Document doc;
            Dictionary<string, object>.Enumerator ienum;

            // Setup
            httpClient = new Http.Client();
            httpGet = new Http.Methods.HttpGet(Utilities.BuildUriForDoc(db, _id));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to synchronously download document.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpGet, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return new Result(false, "Timeout");
                }
                else
                    throw e;
            }

            if (httpResponse != null && httpResponse.ResponseCode != 200)
                return new Result(false, "Resource does not exist.");

            // Read and convert the JSON to a Document
            json = httpResponse.Stream.ReadToEnd();
            doc = ConvertTo.JsonToDocument(json, new DocumentConverter.Get());

            // Assign primary properties
            this._attachments = doc._attachments;
            this._id = doc._id;
            this._rev = doc.Rev;
            this._state = doc.State;

            // Assign user properties
            ienum = doc.GetPropertyEnumerator();
            while (ienum.MoveNext())
            {
                if (ienum.Current.Value.GetType() == typeof(ArrayList))
                { // If it is an ArrayList then it must be handled differently (e.g., ["Tag1", "Tag2"])
                    stringList = new List<string>();
                    al = (ArrayList)ienum.Current.Value;
                    for (int i = 0; i < al.Count; i++)
                    {
                        stringList.Add(al[i].ToString());
                    }
                    _properties.Add(ienum.Current.Key, stringList);
                }
                else
                    _properties.Add(ienum.Current.Key, ienum.Current.Value);
            }

            Logger.General.Debug("Sychronous download of document completed.");

            return new Result(true);
        }

        /// <summary>
        /// Synchronously updates a Document on the Server
        /// </summary>
        /// <param name="db">The Database housing the Document</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Update(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            // Check the _state
            if (!CheckState(CAN_UPDATE))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            string json;
            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpPut httpPut;
            Http.Methods.HttpResponse httpResponse = null;
            byte[] buffer;
            MemoryStream ms = null;

            // Setup
            httpClient = new Http.Client();
            httpPut = new Http.Methods.HttpPut(Utilities.BuildUriForDoc(db, _id), "application/json");
            ms = new MemoryStream();

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            // Make a MemoryStream that contains all the serialized data that represents this Document
            buffer = System.Text.Encoding.UTF8.GetBytes(ConvertTo.AsciiToUtf8(ConvertTo.DocumentToJson(this, new DocumentConverter.Put())));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            Logger.General.Debug("Preparing to synchronously update document.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpPut, ms, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return new Result(false, "Timeout");
                }
                else
                    throw e;
            }

            if (httpResponse == null)
                throw new Http.Network.HttpNetworkException("The response is null.");

            // Read and convert the JSON to a Document
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Assign primary properties
            _id = response.Id;
            _rev = response.Rev;

            Logger.General.Debug("Sychronous update of document completed.");

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Synchronously creates a Document on the Server
        /// </summary>
        /// <param name="db">The Database housing the Document</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Create(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            // Check the _state
            if (!CheckState(CAN_CREATE))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            string json;
            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpPut httpPut;
            Http.Methods.HttpResponse httpResponse = null;
            byte[] buffer;
            MemoryStream ms = null;


            // Setup
            httpClient = new Http.Client();
            httpPut = new Http.Methods.HttpPut(Utilities.BuildUriForDoc(db, _id), "application/json");
            ms = new MemoryStream();

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            // Make a MemoryStream that contains all the serialized data that represents this Document
            buffer = System.Text.Encoding.UTF8.GetBytes(ConvertTo.AsciiToUtf8(ConvertTo.DocumentToJson(this, new DocumentConverter.Put())));
            ms.Write(buffer, 0, buffer.Length);

            Logger.General.Debug("Preparing to synchronously create document.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpPut, ms, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return new Result(false, "Timeout");
                }
                else
                    throw e;
            }

            if (httpResponse == null)
                throw new Http.Network.HttpNetworkException("The response is null.");

            // Read and convert the JSON to a Document
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Assign primary properties
            _id = response.Id;
            _rev = response.Rev;

            Logger.General.Debug("Sychronous creation of document completed.");

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Synchronously deletes a Document on the Server
        /// </summary>
        /// <param name="db">The Database housing the Document</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Delete(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize)
        {
            // Check the _state
            if (!CheckState(CAN_DELETE))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            string json;
            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpDelete httpDelete;
            Http.Methods.HttpResponse httpResponse = null;

            // Setup
            httpClient = new Http.Client();
            httpDelete = new Http.Methods.HttpDelete(Utilities.BuildUriForDoc(db, _id, _rev));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to synchronously delete document.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpDelete, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return new Result(false, "Timeout");
                }
                else
                    throw e;
            }

            if (httpResponse == null)
                throw new Http.Network.HttpNetworkException("The response is null.");

            // Read and convert the JSON to a Document
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Assign primary properties
            _id = response.Id;
            _rev = response.Rev;

            Logger.General.Debug("Sychronous deletion of document completed.");

            // Verify dispatch was successful
            return new Result(response);
        }

        #region Event Handling

        /// <summary>
        /// Called when a download needs to update its progress to any consumers.
        /// </summary>
        /// <param name="sender">The <see cref="Http.Client"/> that is updating.</param>
        /// <param name="connection">The <see cref="Http.Network.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet just received.</param>
        /// <param name="headersTotal">The size of all headers received.</param>
        /// <param name="contentTotal">The size of all content received.</param>
        /// <param name="total">The size of all data received.</param>
        void httpClient_OnDataReceived(Http.Client sender, Http.Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnDownloadProgress != null)
                OnDownloadProgress(this, sender, connection, packetSize, headersTotal, contentTotal, total);
        }

        /// <summary>
        /// Called when a upload needs to update its progress to any consumers.
        /// </summary>
        /// <param name="sender">The <see cref="Http.Client"/> that is updating.</param>
        /// <param name="connection">The <see cref="Http.Network.HttpConnection"/> handling communications.</param>
        /// <param name="packetSize">Size of the packet just sent.</param>
        /// <param name="headersTotal">The size of all headers sent.</param>
        /// <param name="contentTotal">The size of all content sent.</param>
        /// <param name="total">The size of all data sent.</param>
        void httpClient_OnDataSent(Http.Client sender, Http.Network.HttpConnection connection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total)
        {
            if (OnUploadProgress != null)
                OnUploadProgress(this, sender, connection, packetSize, headersTotal, contentTotal, total);
        }

        #endregion



        /// <summary>
        /// Returns the count (number) of properties in this instance
        /// </summary>
        public int PropertyCount
        {
            get { return _properties.Count; }
        }

        /// <summary>
        /// Gets a Dictionary Enumerator for this instance's properties
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object>.Enumerator GetPropertyEnumerator()
        {
            return _properties.GetEnumerator();
        }

        /// <summary>
        /// Gets the property value as a string
        /// </summary>
        /// <param name="key">The key which to get</param>
        /// <returns>A string representation of the value</returns>
        public string GetPropertyAsString(string key)
        {
            if (Contains(key))
                return _properties[key].ToString();

            return null;
        }

        /// <summary>
        /// Gets the property value as an Int64
        /// </summary>
        /// <param name="key">The key which to get</param>
        /// <returns>An Int64 representation of the value</returns>
        public Int64 GetPropertyAsInt64(string key)
        {
            if (Contains(key))
                return Convert.ToInt64(this[key]);

            return -1;
        }

        /// <summary>
        /// Gets the property value as an UInt64
        /// </summary>
        /// <param name="key">The key which to get</param>
        /// <returns>An UInt64 representation of the value</returns>
        public UInt64 GetPropertyAsUInt64(string key)
        {
            if (Contains(key))
                return Convert.ToUInt64(this[key]);

            return 0;
        }

        /// <summary>
        /// Gets the property specified as a nullable DateTime - technically adds the value of ticks to 1/1/1970
        /// </summary>
        /// <param name="key">The key of the property</param>
        /// <returns>A nullable DateTime representation of the value</returns>
        public DateTime? GetPropertyAsDateTime(string key)
        {
            if (Contains(key))
                return DateTime.Parse(this[key].ToString());

            return null;
        }

        /// <summary>
        /// Gets the property as a Dictionary
        /// </summary>
        /// <typeparam name="T">The Type of value expected - if unknown use "object"</typeparam>
        /// <param name="key">The key of the property</param>
        /// <returns>A Dictionary<string, T> representation of the value</returns>
        public Dictionary<string, T> GetPropertyAsDictionary<T>(string key)
        {
            if (Contains(key))
                return (Dictionary<string, T>)this[key];

            return null;
        }

        /// <summary>
        /// Gets the property as a List
        /// </summary>
        /// <typeparam name="T">The Type of value expected - if unknown use "object"</typeparam>
        /// <param name="key">The key of the property</param>
        /// <returns>A List<T> representation of the value</returns>
        public List<T> GetPropertyAsList<T>(string key)
        {
            if (Contains(key))
                return (List<T>)this[key];

            return null;
        }

        public List<T> SetProperty<T>(string key, List<T> value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        /// <summary>
        /// Sets the property to the specified DateTime.
        /// </summary>
        /// <param name="key">The key of the property</param>
        /// <param name="value">The DateTime to set as the value</param>
        /// <returns>An DateTime value that is set to the property</returns>
        public DateTime? SetProperty(string key, DateTime? value)
        {
            DateTime dt;

            if (!value.HasValue)
                return null;

            dt = value.Value.ToUniversalTime();
            string str = dt.ToString("o");

            if (Contains(key))
                _properties[key] = str;
            else
                AddProperty(key, str);

            return value.Value;
        }

        public string SetProperty(string key, string value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public Int16 SetProperty(string key, Int16 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public UInt16 SetProperty(string key, UInt16 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public Int32 SetProperty(string key, Int32 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public UInt32 SetProperty(string key, UInt32 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public Int64 SetProperty(string key, Int64 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }

        public UInt64 SetProperty(string key, UInt64 value)
        {
            if (Contains(key))
                _properties[key] = value;
            else
                AddProperty(key, value);

            return value;
        }
    }
}
