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
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        public delegate void EventHandler(Web.WebState state, Attachment sender);
        /// <summary>
        /// Handles progress events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="percent">The percent.</param>
        /// <param name="bytesSent">The bytes sent.</param>
        /// <param name="bytesTotal">The bytes total.</param>
        public delegate void ProgressEventHandler(Web.WebState state, Attachment sender, decimal percent, Int64 bytesSent, Int64 bytesTotal);
        /// <summary>
        /// Handles completion events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="result">The result.</param>
        public delegate void CompleteEventHandler(Web.WebState state, Attachment sender, Result result);
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
        /// Fired before HTTP headers are locked
        /// </summary>
        public event EventHandler OnBeforeHeadersAreLocked;
        /// <summary>
        /// Fired before the request stream is read and sent
        /// </summary>
        public event EventHandler OnBeforeBeginGetRequestStream;
        /// <summary>
        /// Fired after reading and sending of the request stream is finished
        /// </summary>
        public event EventHandler OnAfterEndGetRequestStream;
        /// <summary>
        /// Fired before attempting to receive the server's response
        /// </summary>
        public event EventHandler OnBeforeBeginGetResponse;
        /// <summary>
        /// Fired after receiving the server's response
        /// </summary>
        public event EventHandler OnAfterEndGetResponse;
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
        /// Fired to indicate that a web transaction is complete
        /// </summary>
        public event CompleteEventHandler OnComplete;

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
            _revpos = 0;
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
            _revpos = 0;
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
                            _state = CAN_DOWNLOAD;
                    }
                    else
                        _state = CAN_DOWNLOAD;
                }
                else if (_stream != null)
                {
                    if (_stream.CanRead)
                    {
                        if (_stream.CanWrite && _revpos > 0 && !string.IsNullOrEmpty(_document.Rev))
                            _state = CAN_ALL;
                        else if (!string.IsNullOrEmpty(_document.Rev))
                            _state = CAN_DELETE | CAN_DOWNLOAD;
                        else
                            _state = CAN_DOWNLOAD;
                    }
                    else if (_stream.CanWrite && _revpos > 0 && !string.IsNullOrEmpty(_document.Rev))
                        _state = CAN_UPLOAD;
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
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="dataStreamMethod">A Web.DataStreamMethod which is used to download the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <param name="use100Continue">Per RFC-2616 "causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data"</param>
        /// <param name="useCompression">Set to true to request data using gzip/deflate when supported by CouchDB</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Stream GetDownloadStreamSync(Server server, Database db, Web.DataStreamMethod dataStreamMethod, bool keepAlive,
            bool use100Continue, bool useCompression)
        {
            // Check the _state
            if (!CheckState(CAN_DOWNLOAD))
                throw new NetException("Invalid state");

            Web web;
            Web.WebState state;
            ServerResponse sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

            Logger.General.Debug("Preparing to establish a synchronous download stream for attachment.");

            // Dispatch the message
            sr = web.SendMessageSync(server, db, _document.Id + "/" + _filename, null, Web.OperationType.GET, dataStreamMethod,
                null, _contentType, keepAlive, use100Continue, useCompression, useCompression, out state);

            Logger.General.Debug("Sychronous download stream for attachment established.");

            return state.Stream;
        }

        /// <summary>
        /// Synchronously downloads an Attachment from the Server to the local system
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="dataStreamMethod">A Web.DataStreamMethod which is used to download the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <param name="use100Continue">Per RFC-2616 "causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data"</param>
        /// <param name="useCompression">Set to true to request data using gzip/deflate when supported by CouchDB</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result DownloadSync(Server server, Database db, Web.DataStreamMethod dataStreamMethod, bool keepAlive,
            bool use100Continue, bool useCompression)
        {
            // Check the _state
            if (!CheckState(CAN_DOWNLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            Web.WebState state;
            ServerResponse sr;
            BinaryReader br;
            BinaryWriter bw;
            int bytesRead;
            long totalBytesRead = 0;
            byte[] buffer;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

            Logger.General.Debug("Preparing to synchronously download attachment.");

            // Dispatch the message
            sr = web.SendMessageSync(server, db, _document.Id + "/" + _filename, null, Web.OperationType.GET, dataStreamMethod,
                null, _contentType, keepAlive, use100Continue, useCompression, useCompression, out state);

            // Setup
            if (_stream == null)
            {
                _stream = new FileStream(_clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
                // Notify consumers
                if (OnFileStreamOpen != null) OnFileStreamOpen(_stream, _clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, "CouchDB.Attachment.Download_OnComplete()");
            }

            bw = new BinaryWriter(_stream);
            br = new BinaryReader(state.Stream);

            buffer = new byte[state.BufferSize];
            while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                bw.Write(buffer, 0, bytesRead);
                //if (OnDownloadProgress != null) OnDownloadProgress(state, this, ((decimal)totalBytesRead / (decimal)state.Stream.Length), totalBytesRead, state.Stream.Length);
                // Above line wont work as cannot seek, must use the attachment's length
                if (OnDownloadProgress != null) OnDownloadProgress(state, this, ((decimal)totalBytesRead / (decimal)this.Length), totalBytesRead, this.Length);
            }

            br.Close();
            bw.Close();
            _stream.Close();

            Logger.General.Debug("Sychronous download of attachment completed.");

            if (OnFileStreamClosed != null) OnFileStreamClosed(_stream, _clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, "CouchDB.Attachment.Download_OnComplete()");

            return new Result(sr);
        }

        /// <summary>
        /// Asynchronously downloads an Attachment from the Server to the local system
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="dataStreamMethod">A Web.DataStreamMethod which is used to download the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <param name="use100Continue">Per RFC-2616 "causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data"</param>
        /// <param name="useCompression">Set to true to request data using gzip/deflate when supported by CouchDB</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Download(Server server, Database db, Web.DataStreamMethod dataStreamMethod, bool keepAlive,
            bool use100Continue, bool useCompression)
        {
            // Check the _state
            if (!CheckState(CAN_DOWNLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            ServerResponse sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnAfterEndGetRequestStream += new Web.MessageHandler(web_OnAfterEndGetRequestStream);
            web.OnAfterEndGetResponse += new Web.MessageHandler(web_OnAfterEndGetResponse);
            web.OnBeforeBeginGetRequestStream += new Web.MessageHandler(web_OnBeforeBeginGetRequestStream);
            web.OnBeforeBeginGetResponse += new Web.MessageHandler(web_OnBeforeBeginGetResponse);
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnComplete += new Web.MessageHandler(Download_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);
            
            Logger.General.Debug("Preparing to asynchronously download attachment.");

            // Dispatch the message
            sr = web.SendMessage(server, db, _document.Id + "/" + _filename, null, Web.OperationType.GET, dataStreamMethod,
                null, _contentType, keepAlive, use100Continue, useCompression, useCompression);

            Logger.General.Debug("Asychronous download of attachment completed.");

            // Verify dispatch was successful
            return new Result(sr);
        }

        /// <summary>
        /// Synchronously uploads an Attachment to a Server from the local system
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="dataStreamMethod">A Web.DataStreamMethod which is used to download the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <param name="use100Continue">Per RFC-2616 "causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data"</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result UploadSync(Server server, Database db, Web.DataStreamMethod dataStreamMethod, bool keepAlive,
            bool use100Continue)
        {
            // Check the _state
            if (!CheckState(CAN_UPLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            Web.WebState state;
            string json;
            ServerResponse response;
            StreamReader sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

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

            // Dispatch the message - AFAIK CouchDB does not support compression in PUT
            response = web.SendMessageSync(server, db, _document.Id + "/" + _filename, "rev=" + _document.Rev, Web.OperationType.PUT, dataStreamMethod,
                _stream, _contentType, keepAlive, use100Continue, false, false, out state);

            Logger.General.Debug("Sychronous upload of attachment completed.");
            
            // Setup
            sr = new StreamReader(state.Stream);

            // Get the response and deserialize it
            json = sr.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Update the housing Document
            _document.Rev = response.Rev;

            // Update the revision position
            _revpos = Convert.ToInt32(response.Rev.Substring(0, response.Rev.IndexOf('-')));

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Asynchronously uploads an Attachment to a Server from the local system
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="dataStreamMethod">A Web.DataStreamMethod which is used to download the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <param name="use100Continue">Per RFC-2616 "causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data"</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Upload(Server server, Database db, Web.DataStreamMethod dataStreamMethod, bool keepAlive,
            bool use100Continue)
        {
            // Check the _state
            if (!CheckState(CAN_UPLOAD))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            ServerResponse sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnAfterEndGetRequestStream += new Web.MessageHandler(web_OnAfterEndGetRequestStream);
            web.OnAfterEndGetResponse += new Web.MessageHandler(web_OnAfterEndGetResponse);
            web.OnBeforeBeginGetRequestStream += new Web.MessageHandler(web_OnBeforeBeginGetRequestStream);
            web.OnBeforeBeginGetResponse += new Web.MessageHandler(web_OnBeforeBeginGetResponse);
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnComplete += new Web.MessageHandler(Upload_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

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

            Logger.General.Debug("Preparing to asynchronously upload attachment.");

            // Dispatch the message - AFAIK CouchDB does not support compression in PUT
            sr = web.SendMessage(server, db, _document.Id + "/" + _filename, "rev=" + _document.Rev, Web.OperationType.PUT, dataStreamMethod,
                _stream, _contentType, keepAlive, use100Continue, false, false);

            Logger.General.Debug("Asychronous upload of attachment completed.");

            // Verify dispatch was successful
            return new Result(sr);
        }

        /// <summary>
        /// Synchronously deletes an Attachment on the Server
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result DeleteSync(Server server, Database db, bool keepAlive)
        {
            // Check the _state
            if (!CheckState(CAN_DELETE))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            Web.WebState state;
            string json;
            ServerResponse response;
            StreamReader sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

            Logger.General.Debug("Preparing to synchronously delete attachment.");

            // Dispatch the message - AFAIK CouchDB does not support compression in PUT
            response = web.SendMessageSync(server, db, _document.Id + "/" + _filename, "rev=" + _document.Rev, Web.OperationType.DELETE, Web.DataStreamMethod.LoadToMemory,
                null, _contentType, keepAlive, false, false, false, out state);

            Logger.General.Debug("Sychronous delete of attachment completed.");
            
            // Setup
            sr = new StreamReader(state.Stream);

            json = sr.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            _document.Rev = response.Rev;

            // Verify dispatch was successful
            return new Result(response);
        }

        /// <summary>
        /// Asynchronously deletes an Attachment on the Server
        /// </summary>
        /// <param name="server">The Server housing the Database</param>
        /// <param name="db">The Database housing the Document which houses the Attachment</param>
        /// <param name="keepAlive">True if the connection should be kept alive for further requests</param>
        /// <returns>A CouchDB.Result representing the result of the request</returns>
        public Result Delete(Server server, Database db, bool keepAlive)
        {
            // Check the _state
            if (!CheckState(CAN_DELETE))
                return new Result(false, "Invalid state", Result.INVALID_STATE);

            Web web;
            ServerResponse sr;

            // Setup
            web = new Web();

            // Setup Event Handlers
            web.OnAfterEndGetRequestStream += new Web.MessageHandler(web_OnAfterEndGetRequestStream);
            web.OnAfterEndGetResponse += new Web.MessageHandler(web_OnAfterEndGetResponse);
            web.OnBeforeBeginGetRequestStream += new Web.MessageHandler(web_OnBeforeBeginGetRequestStream);
            web.OnBeforeBeginGetResponse += new Web.MessageHandler(web_OnBeforeBeginGetResponse);
            web.OnBeforeHeadersAreLocked += new Web.MessageHandler(web_OnBeforeHeadersAreLocked);
            web.OnComplete += new Web.MessageHandler(Delete_OnComplete);
            web.OnTimeout += new Web.MessageHandler(web_OnTimeout);
            web.OnUploadProgress += new Web.MessageProgressHandler(web_OnUploadProgress);

            Logger.General.Debug("Preparing to asynchronously delete attachment.");

            // Dispatch the message - AFAIK CouchDB does not support compression in PUT
            sr = web.SendMessage(server, db, _document.Id + "/" + _filename, "rev=" + _document.Rev, Web.OperationType.DELETE, Web.DataStreamMethod.LoadToMemory,
                null, _contentType, keepAlive, false, false, false);

            Logger.General.Debug("Asychronous delete of attachment completed.");

            // Verify dispatch was successful
            return new Result(sr);
        }

        #region Event Handling

        /// <summary>
        /// Called when Upload needs to update its progress to any consumers
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        /// <param name="percent">The percentage of the file transfered to the server</param>
        /// <param name="bytesSent">The bytes sent</param>
        /// <param name="bytesTotal">The total amount of bytes to be sent</param>
        void web_OnUploadProgress(Web.WebState state, decimal percent, Int64 bytesSent, Int64 bytesTotal)
        {
            if (OnUploadProgress != null) OnUploadProgress(state, this, percent, bytesSent, bytesTotal);
        }

        /// <summary>
        /// Called by Download, Upload or Delete if a timeout occurs to notify any consumers
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnTimeout(Web.WebState state)
        {
            if (OnTimeout != null) OnTimeout(state, this);
        }

        /// <summary>
        /// Called by Download to write the file to the local system and notify any consumers upon completion of writing the file
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void Download_OnComplete(Web.WebState state)
        {
            BinaryReader br;
            BinaryWriter bw;
            int bytesRead;
            long totalBytesRead = 0;
            byte[] buffer;

            // Setup
            if (_stream == null)
            {
                _stream = new FileStream(_clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None);
                // Notify consumers
                if (OnFileStreamOpen != null) OnFileStreamOpen(_stream, _clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, "CouchDB.Attachment.Download_OnComplete()");
            }

            bw = new BinaryWriter(_stream);
            br = new BinaryReader(state.Stream);

            buffer = new byte[state.BufferSize];
            while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesRead += bytesRead;
                bw.Write(buffer, 0, bytesRead);
                //if (OnDownloadProgress != null) OnDownloadProgress(state, this, ((decimal)totalBytesRead / (decimal)state.Stream.Length), totalBytesRead, state.Stream.Length);
                // Above line wont work as cannot seek, must use the attachment's length
                if (OnDownloadProgress != null) OnDownloadProgress(state, this, ((decimal)totalBytesRead / (decimal)this.Length), totalBytesRead, this.Length);
            }

            br.Close();
            bw.Close();
            _stream.Close();
            if (OnFileStreamClosed != null) OnFileStreamClosed(_stream, _clientFilepath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, "CouchDB.Attachment.Download_OnComplete()");

            if (OnComplete != null) OnComplete(state, this, new Result(true));
        }

        /// <summary>
        /// Called by Upload to notify any consumers upon completion of uploading the file
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void Upload_OnComplete(Web.WebState state)
        {
            string json;
            ServerResponse response;
            StreamReader sr;

            // Setup
            sr = new StreamReader(state.Stream);

            // Get the response and deserialize it
            json = sr.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            // Update the housing Document
            _document.Rev = response.Rev;

            // Update the revision position
            _revpos = Convert.ToInt32(response.Rev.Substring(0, response.Rev.IndexOf('-')));
            
            
            // Notify any consumers
            if (OnComplete != null) OnComplete(state, this, new Result(response));
        }

        /// <summary>
        /// Called by Delete to notify any consumers upon completion of deleting the attachment
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void Delete_OnComplete(Web.WebState state)
        {
            string json;
            ServerResponse response;
            StreamReader sr;

            // Setup
            sr = new StreamReader(state.Stream);

            json = sr.ReadToEnd();
            response = ConvertTo.JsonToServerResponse(json);

            _document.Rev = response.Rev;
            if (OnComplete != null) OnComplete(state, this, new Result(response));
        }

        /// <summary>
        /// Called by any network method to notify consumers that the HTTP headers are about to be locked
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeHeadersAreLocked(Web.WebState state)
        {
            if (OnBeforeHeadersAreLocked != null) OnBeforeHeadersAreLocked(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that receiving of the server's response is about to begin
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeBeginGetResponse(Web.WebState state)
        {
            if (OnBeforeBeginGetResponse != null) OnBeforeBeginGetResponse(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that the request stream is about to be read and sent
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnBeforeBeginGetRequestStream(Web.WebState state)
        {
            if (OnBeforeBeginGetRequestStream != null) OnBeforeBeginGetRequestStream(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that the server's response has been received
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnAfterEndGetResponse(Web.WebState state)
        {
            if (OnAfterEndGetResponse != null) OnAfterEndGetResponse(state, this);
        }

        /// <summary>
        /// Called by any network method to notify consumers that reading and sending of the request stream are finished
        /// </summary>
        /// <param name="state">The Web.WebState instance</param>
        void web_OnAfterEndGetRequestStream(Web.WebState state)
        {
            if (OnAfterEndGetRequestStream != null) OnAfterEndGetRequestStream(state, this);
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