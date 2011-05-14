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
using System.Net;

namespace Common.CouchDB
{
    /// <summary>
    /// The Attachment class is used to represent a CouchDB Attachment
    /// </summary>
    public class Attachment
    {
        private const int RESET = 0x00;
        private const int CAN_DOWNLOAD = 0x01;
        private const int CAN_UPLOAD = 0x02;
        private const int CAN_DELETE = 0x04;
        private const int CAN_ALL = CAN_DOWNLOAD | CAN_UPLOAD | CAN_DELETE;
        private const int CAN_GET_DOWNLOAD_STREAM = 0x08;

        /// <summary>
        /// The state of the instance, can be any combination of: RESET, CAN_DOWNLOAD, CAN_UPLOAD, CAN_DELETE, CAN_ALL
        /// </summary>
        private int _state;

        /// <summary>
        /// The remote filename of the Attachment
        /// </summary>
        private string _filename;

        /// <summary>
        /// The local filename of the Attachment
        /// </summary>
        private string _clientFilepath;

        /// <summary>
        /// The content type of the Attachment
        /// </summary>
        private string _contentType;

        /// <summary>
        /// The length of the Attachment in bytes
        /// </summary>
        private long _length;

        /// <summary>
        /// The position of the Attachment in the CouchDB's version history
        /// </summary>
        private int _revpos;

        /// <summary>
        /// The Stream that is used to either read or write the file
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// The Document containing the Attachment
        /// </summary>
        private Document _document;

        /// <summary>
        /// Handles general events
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="httpClient">The <see cref="Http.Client"/> handling communications.</param>
        public delegate void EventHandler(Attachment sender, Http.Client httpClient);
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
        public delegate void ProgressEventHandler(Attachment sender, Http.Client httpClient, Http.Network.HttpConnection httpConnection, int packetSize, ulong headersTotal, ulong contentTotal, ulong total);
        /// <summary>
        /// Handle file events
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="access">The access.</param>
        /// <param name="share">The share.</param>
        /// <param name="options">The options.</param>
        /// <param name="owner">The owner.</param>
        public delegate void FileEventHandler(Stream stream, string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, string owner);

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

        /// <summary>
        /// Fired when a FileStream is opened
        /// </summary>
        public event FileEventHandler OnFileStreamOpen;

        /// <summary>
        /// Fired when a FileStream is closed
        /// </summary>
        public event FileEventHandler OnFileStreamClosed;

        #region Properties

        /// <summary>
        /// Gets/Sets the filename of the attachment
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set 
            { 
                _filename = value;
                ResetState();
            }
        }

        /// <summary>
        /// Gets/Sets the path of the file on the local system for the attachment
        /// </summary>
        public string ClientFilepath
        {
            get { return _clientFilepath; }
            set
            {
                _clientFilepath = value;
                ResetState();
            }
        }

        /// <summary>
        /// Gets/Sets the content type of the attachment
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set
            {
                _contentType = value;
                ResetState();
            }
        }

        /// <summary>
        /// Gets/Sets the length of the attachment
        /// </summary>
        public long Length
        {
            get { return _length; }
            set { _length = value; }
        }

        /// <summary>
        /// Gets the position of the attachment in the revision history
        /// </summary>
        public int Revpos
        {
            get { return _revpos; }
        }

        /// <summary>
        /// Gets/Sets the underlying attachment's stream
        /// </summary>
        public Stream Stream
        {
            get { return _stream; }
            set 
            { 
                _stream = value;
                ResetState();
            }
        }

        /// <summary>
        /// Gets/Sets the Document housing this Attachment
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set
            {
                _document = value;
                ResetState();
            }
        }

        #endregion
        
        #region Constructors & Instantiation

        /// <summary>
        /// Null Constructor
        /// </summary>
        public Attachment()
        {
            //_state = RESET;
            _filename = null;
            _clientFilepath = null;
            _contentType = null;
            _length = 0;
            _revpos = 0;
            _stream = null;
            _document = null;
            ResetState();

            Logger.General.Debug("Common.CouchDB.Attachment instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The filename of the attachment</param>
        /// <param name="contenttype">The content type of the attachment</param>
        /// <param name="length">The length of the attachment</param>
        /// <param name="doc">The parent Document</param>
        public Attachment(string filename, string contenttype, long length, Document doc)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null or empty", "filename");
            if (string.IsNullOrEmpty(contenttype))
                throw new ArgumentException("contenttype cannot be null or empty", "contenttype");
            if (length < 0)
                throw new ArgumentException("length cannot negative", "length");
            if (doc == null)
                throw new ArgumentException("doc cannot be null", "doc");

            //_state = CAN_DOWNLOAD | CAN_DELETE;
            _filename = filename;
            _clientFilepath = null;
            _contentType = contenttype;
            _length = length;
            _revpos = Convert.ToInt32(doc.Rev.Substring(0, doc.Rev.IndexOf('-')));
            _stream = null;
            _document = doc;
            ResetState();

            Logger.General.Debug("Common.CouchDB.Attachment instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The filename of the attachment</param>
        /// <param name="contenttype">The content type of the attachment</param>
        /// <param name="length">The length of the attachment</param>
        /// <param name="revpos">The position of the attachment in the revision history</param>
        /// <param name="doc">The parent Document</param>
        public Attachment(string filename, string contenttype, long length, int revpos, Document doc)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null or empty", "filename");
            if (string.IsNullOrEmpty(contenttype))
                throw new ArgumentException("contenttype cannot be null or empty", "contenttype");
            if (length < 0)
                throw new ArgumentException("length cannot negative", "length");
            if (doc == null)
                throw new ArgumentException("doc cannot be null", "doc");

            //_state = CAN_DOWNLOAD | CAN_DELETE;
            _filename = filename;
            _clientFilepath = null;
            _contentType = contenttype;
            _length = length;
            _revpos = revpos;
            _stream = null;
            _document = doc;
            ResetState();

            Logger.General.Debug("Common.CouchDB.Attachment instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The filename of the attachment</param>
        /// <param name="contenttype">The content type of the attachment</param>
        /// <param name="length">The length of the attachment</param>
        /// <param name="stream">The underlying attachment's stream</param>
        /// <param name="doc">The parent Document</param>
        public Attachment(string filename, string contenttype, long length, Stream stream, Document doc)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null or empty", "filename");
            if (string.IsNullOrEmpty(contenttype))
                throw new ArgumentException("contenttype cannot be null or empty", "contenttype");
            if (length < 0)
                throw new ArgumentException("length cannot negative", "length");
            if (stream == null)
                throw new ArgumentException("stream cannot be null", "stream");
            if (doc == null)
                throw new ArgumentException("doc cannot be null", "doc");

            //_state = CAN_ALL;
            _filename = filename;
            _clientFilepath = null;
            _contentType = contenttype;
            _length = length;
            _revpos = Convert.ToInt32(doc.Rev.Substring(0, doc.Rev.IndexOf('-')));
            _stream = stream;
            _document = doc;
            ResetState();

            Logger.General.Debug("Common.CouchDB.Attachment instantiated.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The filename of the attachment</param>
        /// <param name="clientFilepath">The path of the file on the client system</param>
        /// <param name="contenttype">The content type of the attachment</param>
        /// <param name="length">The length of the attachment</param>
        /// <param name="stream">The underlying attachment's stream</param>
        /// <param name="doc">The parent Document</param>
        public Attachment(string filename, string clientFilepath, string contenttype, long length, Stream stream, Document doc)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename cannot be null or empty", "filename");
            if (string.IsNullOrEmpty(clientFilepath))
                throw new ArgumentException("clientFilepath cannot be null or empty", "filename");
            if (string.IsNullOrEmpty(contenttype))
                throw new ArgumentException("contenttype cannot be null or empty", "contenttype");
            if (length < 0)
                throw new ArgumentException("length cannot negative", "length");
            if (stream == null)
                throw new ArgumentException("stream cannot be null", "stream");
            if (doc == null)
                throw new ArgumentException("doc cannot be null", "doc");

            //_state = CAN_ALL;
            _filename = filename;
            _clientFilepath = clientFilepath;
            _contentType = contenttype;
            _length = length;
            _revpos = 0;
            _stream = stream;
            _document = doc;
            ResetState();

            Logger.General.Debug("Common.CouchDB.Attachment instantiated.");
        }

        /// <summary>
        /// Creates a new instance of an attachment and returns the created instance, this method should only be used when FETCHING attachments from CouchDB as it has neither a _stream nor a 
        /// _clientFilepath it cannot be used to create an instance of an Attachment capable of being uploaded unless that property is independently set
        /// </summary>
        /// <param name="doc">The instantiated document with the Attachment's "stub" as returned by CouchDB</param>
        /// <param name="kvp">A KeyValuePair having an entry key=filename,value=CouchDB stub where CouchDB stub has keys and values for content_type, length and revpos</param>
        /// <returns>An instance of the created Attachment</returns>
        public static Attachment Instantiate(Document doc, KeyValuePair<string, object> kvp)
        {
            Dictionary<string, object> dict;
            Attachment att;
            string filename, contenttype;
            long length;
            int revpos;

            dict = (Dictionary<string, object>)kvp.Value;

            if (kvp.Key.GetType() != typeof(string))
                throw new CouchDBException("Key must be of type string");

            if (!dict.ContainsKey("content_type") || dict["content_type"].GetType() != typeof(string))
                throw new CouchDBException("The contained dictionary must contain a key for content_type and its value must be of type string");

            if (!dict.ContainsKey("length") || (dict["length"].GetType() != typeof(long) && dict["length"].GetType() != typeof(int)))
                throw new CouchDBException("The contained dictionary must contain a key for length and its value must be of type long or int");

            if (!dict.ContainsKey("revpos") || dict["revpos"].GetType() != typeof(int))
                throw new CouchDBException("The contained dictionary must contain a key for revpos and its value must be of type int");

            filename = (string)kvp.Key;
            contenttype = (string)dict["content_type"];
            length = Convert.ToInt64(dict["length"]);
            revpos = Convert.ToInt32(dict["revpos"]);

            att = new Attachment(filename, contenttype, length, revpos, doc);

            return att;
        }
                
        #endregion

        /// <summary>
        /// Resets the _state property accordingly to the properties currently set within the instance
        /// </summary>
        private void ResetState()
        {
            _state = RESET;

            // To download or delete need a document instance, filename, content type
            // To upload need a document, filename, content type, and either a client filepath or a stream
            if (string.IsNullOrEmpty(_document.Id))
                return;

            if (!string.IsNullOrEmpty(_filename) && _document != null)
            {
                if (!string.IsNullOrEmpty(_clientFilepath)) // Is there a string
                {
                    if (System.IO.File.Exists(_clientFilepath)) // Does it actually exist
                    {
                        if (!string.IsNullOrEmpty(_document.Rev) && !string.IsNullOrEmpty(_contentType)) // Do we know where to go remotely for Delete and Upload?
                            _state = CAN_ALL;
                        else
                            _state = CAN_DOWNLOAD | CAN_GET_DOWNLOAD_STREAM;
                    }
                    else
                        _state = CAN_DOWNLOAD | CAN_GET_DOWNLOAD_STREAM;
                }
                else if (_stream != null)
                {
                    if (_stream.CanRead)
                    {
                        if (_stream.CanWrite)
                        {
                            if (!string.IsNullOrEmpty(_document.Rev))
                            {
                                // Revision position is known - meaning I can write to CouchDB
                                if (_revpos <= 0)
                                    _revpos = Convert.ToInt32(_document.Rev.Substring(0, _document.Rev.IndexOf('-')));

                                _state = CAN_ALL | CAN_GET_DOWNLOAD_STREAM;
                            }
                            else
                                _state = CAN_DOWNLOAD | CAN_GET_DOWNLOAD_STREAM;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_document.Rev))
                            {
                                // Revision position is known - meaning I can write to CouchDB
                                if (_revpos <= 0)
                                    _revpos = Convert.ToInt32(_document.Rev.Substring(0, _document.Rev.IndexOf('-')));

                                _state = CAN_UPLOAD | CAN_DELETE | CAN_GET_DOWNLOAD_STREAM;
                            }
                        }
                    }
                    else if (_stream.CanWrite)
                    {
                        if (!string.IsNullOrEmpty(_document.Rev))
                        {
                            // I cannot read from the stream, thus I cannot send data, but I can receive it

                            _state = CAN_DOWNLOAD | CAN_DELETE | CAN_GET_DOWNLOAD_STREAM;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(_document.Rev))
                {
                    _state = CAN_GET_DOWNLOAD_STREAM;
                }
            }
        }

        /// <summary>
        /// True if the Attachment can be downloaded
        /// </summary>
        public bool CanDownload
        {
            get { return CheckState(CAN_DOWNLOAD); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can get a download stream.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can get a download stream; otherwise, <c>false</c>.
        /// </value>
        public bool CanGetDownloadStream
        {
            get { return CheckState(CAN_GET_DOWNLOAD_STREAM); }
        }

        /// <summary>
        /// True if the Attachment can be uploaded
        /// </summary>
        public bool CanUpload
        {
            get { return CheckState(CAN_UPLOAD); }
        }

        /// <summary>
        /// True if the Attachment can be deleted
        /// </summary>
        public bool CanDelete
        {
            get { return CheckState(CAN_DELETE); }
        }

        /// <summary>
        /// Checks to see if the current _state of the Attachment supports the requested action
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
        /// Synchronously accesses the CouchDB server and gets a Stream that can access the network data being received from the CouchDB server
        /// </summary>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="job">The <see cref="Work.JobBase"/>.</param>
        /// <returns>
        /// A <see cref="Http.Network.HttpNetworkStream"/> for the content.
        /// </returns>
        public Http.Network.HttpNetworkStream GetDownloadStream(Database db, int sendTimeout, 
            int receiveTimeout, int sendBufferSize, int receiveBufferSize, Work.JobBase job)
        {
            // Check the _state
            if (!CheckState(CAN_GET_DOWNLOAD_STREAM))
                throw new NetException("Invalid state");

            Http.Client httpClient;
            Http.Methods.HttpGet httpGet;
            Http.Methods.HttpResponse httpResponse = null;

            // Setup
            httpClient = new Http.Client();
            httpGet = new Http.Methods.HttpGet(Utilities.BuildUriForAttachment(db, _document.Id, _filename));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to establish a synchronous download stream for attachment.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpGet, null, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize, job);
            }
            catch (Http.Network.HttpNetworkTimeoutException e)
            {
                if (OnTimeout != null)
                {
                    OnTimeout(this, httpClient);
                    return null;
                }
                else
                    throw e;
            }

            if (httpResponse != null && httpResponse.ResponseCode == 200)
            {
                Logger.General.Debug("Sychronous download stream for attachment established.");
                _length = (long)Http.Utilities.GetContentLength(httpGet.Headers);
                return httpResponse.Stream;
            }
            else
            {
                Logger.General.Debug("Failed to get a download stream for the attachment.");
                return null;
            }
        }

        /// <summary>
        /// Synchronously uploads an Attachment to a Server from the local system
        /// </summary>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="job">The <see cref="Work.JobBase"/>.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Upload(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize, Work.JobBase job)
        {
            // Check the _state
            if (!CheckState(CAN_UPLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            string json;
            ServerResponse response;
            Http.Client httpClient;
            Http.Methods.HttpPut httpPut;
            Http.Methods.HttpResponse httpResponse = null;

            // Setup
            httpClient = new Http.Client();
            httpPut = new Http.Methods.HttpPut(Utilities.BuildUriForAttachment(db, _document.Id, _filename, _document.Rev), _contentType);

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            // Check to see if the _stream is null - stream takes priority
            if (_stream == null)
            {
                // _stream is null so see if _clientFilepath exists
                if (!File.Exists(_clientFilepath))
                    return new Result(false, "Invalid state - neither _stream nor _clientFilepath are correctly set", Result.INVALID_STATE);

                // Create a new FileStream based on the _clientFilepath
                _stream = new FileStream(_clientFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Send to event handler if a consumer exists
                if (OnFileStreamOpen != null) OnFileStreamOpen(_stream, _clientFilepath, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, "CouchDB.Attachment.Upload()");
            }

            Logger.General.Debug("Preparing to synchronously upload attachment.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpPut, _stream, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize, job);
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
            {
                if (job.IsCancelled)
                    return new Result(false, "Canceled by user.");
                else
                    throw new Http.Network.HttpNetworkException("The response is null.");
            }

            Logger.General.Debug("Sychronous upload of attachment completed.");
            
            // Get the response and deserialize it
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Update the housing Document
            _document.Rev = response.Rev;

            // Update the revision position
            _revpos = Convert.ToInt32(response.Rev.Substring(0, response.Rev.IndexOf('-')));

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Synchronously deletes an Attachment on the Server
        /// </summary>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="sendTimeout">The send timeout.</param>
        /// <param name="receiveTimeout">The receive timeout.</param>
        /// <param name="sendBufferSize">Size of the send buffer.</param>
        /// <param name="receiveBufferSize">Size of the receive buffer.</param>
        /// <param name="job">The <see cref="Work.JobBase"/>.</param>
        /// <returns>
        /// A CouchDB.Result representing the result of the request
        /// </returns>
        public Result Delete(Database db, int sendTimeout, int receiveTimeout, int sendBufferSize, int receiveBufferSize, Work.JobBase job)
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
            httpDelete = new Http.Methods.HttpDelete(Utilities.BuildUriForAttachment(db, _document.Id, _filename, _revpos));

            // Setup Event Handlers
            httpClient.OnDataReceived += new Http.Client.DataReceivedDelegate(httpClient_OnDataReceived);
            httpClient.OnDataSent += new Http.Client.DataSentDelegate(httpClient_OnDataSent);

            Logger.General.Debug("Preparing to synchronously delete attachment.");

            // Dispatch the message
            try
            {
                httpResponse = httpClient.Execute(httpDelete, _stream, sendTimeout, receiveTimeout, sendBufferSize, receiveBufferSize, job);
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

            Logger.General.Debug("Sychronous delete of attachment completed.");

            // Read and convert the JSON to a Document
            json = httpResponse.Stream.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            _document.Rev = response.Rev;

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
        /// Serializes this Attachment
        /// </summary>
        /// <returns>A KeyValuePair representing this Attachment</returns>
        public KeyValuePair<string, object> Serialize()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(_contentType))
                throw new CouchDBException("The property _contentType cannot be a null or empty value");
            if (_revpos <= 0)
                throw new CouchDBException("The property _revpos must be a positive value (greater than 0)");
            if (_length < 0)
                throw new CouchDBException("The property _length must be greater than or equal to 0");

            dictionary.Add("content_type", _contentType);
            dictionary.Add("revpos", _revpos);
            dictionary.Add("length", _length);
            dictionary.Add("stub", true);

            return new KeyValuePair<string, object>(_filename, dictionary);
        }

        /// <summary>
        /// Closes the _stream and resets the _state
        /// </summary>
        public void CloseStream()
        {
            if (_stream != null)
                _stream.Close();

            ResetState();
        }

        /// <summary>
        /// Destructor - closes the _stream
        /// </summary>
        ~Attachment()
        {
            if (_stream != null)
                _stream.Close();
        }
    }
}