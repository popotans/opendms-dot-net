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
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;

namespace Common.CouchDB
{
    /// <summary>
    /// Provides the web (HTTP) communications medium for CouchDB.
    /// </summary>
    public class Web
    {
        #region Notes

        // HttpWebRequest.Timeout property has no effect on Async calls, we must implement our own
        // http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.timeout%28v=VS.90%29.aspx

        // http://www.w3.org/Protocols/rfc2616/rfc2616-sec8.html 8.2.3 Use of the 100 (Continue) Status
        // 100-Continue causes the client to wait for a response, this is done to allow the server time to interpret the headers and determine if it will accept the request before sending the data.

        // http://en.wikipedia.org/wiki/Chunked_transfer_encoding Chunked transfer encoding
            

        // This library makes use of CouchDB's Standalone Attachments.  These attachments must be sent and received as binary data.
        // Therefore, below is an example on how to read a returned stream.
        /* 
        --- USAGE EXAMPLE ---
        BinaryReader br = new BinaryReader(att.Stream);
        FileStream fs = new FileStream("C:\\Test.doc", FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        byte[] buffer = new byte[1024];
        int bytesRead = 0;

        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
        {
            bw.Write(buffer, 0, bytesRead);
        }

        br.Close();
        bw.Close();
        */
       

        #endregion

        #region Enums and Classes used within Web

        /// <summary>
        /// Types of operations
        /// </summary>
        public enum OperationType
        {
            /// <summary>
            /// No operation
            /// </summary>
            NULL,
            /// <summary>
            /// Receiving a resource from CouchDB
            /// </summary>
            GET,
            /// <summary>
            /// Sending a resource to CouchDB
            /// </summary>
            PUT,
            /// <summary>
            /// Posting a resource (not often used, you should consider using PUT unless you are sure you want to POST)
            /// </summary>
            POST,
            /// <summary>
            /// Deleting a resource
            /// </summary>
            DELETE,
            /// <summary>
            /// Copying one resource to another
            /// </summary>
            COPY
        }

        /// <summary>
        /// How data should be handled in respect to memory usage
        /// </summary>
        public enum DataStreamMethod
        {
            /// <summary>
            /// Saves all the data into memory then does stream operations
            /// </summary>
            /// <remarks>Faster than Stream, but potentially fatal memory requirements.</remarks>
            LoadToMemory,
            /// <summary>
            /// Chunks data from the file system in chunks that can be managed.
            /// </summary>
            /// <remarks>Slower than LoadToMemory, but safe for large data files.</remarks>
            Stream
        }

        /// <summary>
        /// Represents the state of a <see cref="Web"/> instance.
        /// </summary>
        public class WebState
        {
            /// <summary>
            /// The <see cref="HttpWebRequest"/>
            /// </summary>
            public HttpWebRequest Request;
            /// <summary>
            /// <see cref="HttpWebResponse"/>
            /// </summary>
            public HttpWebResponse Response;
            /// <summary>
            /// <see cref="DataStreamMethod"/>
            /// </summary>
            public DataStreamMethod DataStreamMethod;
            /// <summary>
            /// The size (bytes) of the network buffer
            /// </summary>
            public int BufferSize;
            /// <summary>
            /// <see cref="OperationType"/>
            /// </summary>
            public OperationType Operation;
            /// <summary>
            /// <see cref="RegisteredWaitHandle"/>
            /// </summary>
            public RegisteredWaitHandle TimeoutRegisteredWaitHandle;
            /// <summary>
            /// <see cref="WaitHandle"/>
            /// </summary>
            public WaitHandle TimeoutWaitHandle;
            /// <summary>
            /// <c>True</c> if a timeout occurred; otherwise, <c>false</c>.
            /// </summary>
            public bool IsTimeout;
            /// <summary>
            /// The amount of time (milliseconds) to elapse before firing a timeout event
            /// </summary>
            public int TimeoutDuration;
            /// <summary>
            /// The <see cref="Stream"/> to use for streamie things
            /// </summary>
            public Stream Stream;
            /// <summary>
            /// <c>True</c> if the stream is binary; otherwise, <c>false</c>.
            /// </summary>
            public bool StreamIsBinary;

            /// <summary>
            /// Initializes a new instance of the <see cref="WebState"/> class.
            /// </summary>
            public WebState()
            {
                Request = null;
                Response = null;
                DataStreamMethod = Web.DataStreamMethod.Stream;
                BufferSize = -1;
                Operation = OperationType.NULL;
                TimeoutRegisteredWaitHandle = null;
                TimeoutWaitHandle = null;
                IsTimeout = false;
                TimeoutDuration = -1;
                Stream = null;
                StreamIsBinary = false;
            }
        }

        #endregion

        #region Events & Delegates

        /// <summary>
        /// Handles general message events
        /// </summary>
        /// <param name="state">The state.</param>
        public delegate void MessageHandler(WebState state);
        /// <summary>
        /// Handles progress message events
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="percent">The percent.</param>
        /// <param name="bytesSent">The bytes sent.</param>
        /// <param name="bytesTotal">The bytes total.</param>
        public delegate void MessageProgressHandler(WebState state, decimal percent, Int64 bytesSent, Int64 bytesTotal);

        /// <summary>
        /// Occurs before the headers are locked.
        /// </summary>
        public event MessageHandler OnBeforeHeadersAreLocked;
        /// <summary>
        /// Occurs befire BeginGetRequestStream is called.
        /// </summary>
        public event MessageHandler OnBeforeBeginGetRequestStream;
        /// <summary>
        /// Occurs after EndGetRequestStream is called.
        /// </summary>
        public event MessageHandler OnAfterEndGetRequestStream;
        /// <summary>
        /// Occurs before BeginGetResponse is called.
        /// </summary>
        public event MessageHandler OnBeforeBeginGetResponse;
        /// <summary>
        /// Occurs after EndGetResponse is called.
        /// </summary>
        public event MessageHandler OnAfterEndGetResponse;
        /// <summary>
        /// Occurs when a message completes.
        /// </summary>
        public event MessageHandler OnComplete;
        /// <summary>
        /// Occurs when a timeout occurrs.
        /// </summary>
        public event MessageHandler OnTimeout;
        /// <summary>
        /// Occurs when uploading progress is updated.
        /// </summary>
        public event MessageProgressHandler OnUploadProgress;


        #endregion

        /// <summary>
        /// Synchronously gets the request stream.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="db">The db.</param>
        /// <param name="path">The path.</param>
        /// <param name="query">The query.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="dataStreamMethod">The data stream method.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <param name="keepAlive">if set to <c>true</c> [keep alive].</param>
        /// <param name="use100Continue">if set to <c>true</c> [use100 continue].</param>
        /// <param name="useDeflate">if set to <c>true</c> [use deflate].</param>
        /// <param name="useGzip">if set to <c>true</c> [use gzip].</param>
        /// <returns></returns>
        public Stream GetRequestStreamForMessageSync(Server server, Database db, string path, string query, OperationType operation,
            DataStreamMethod dataStreamMethod, string contentType, long contentLength, bool keepAlive, bool use100Continue,
            bool useDeflate, bool useGzip)
        {
            if (server == null)
                throw new ArgumentException("server cannot be null", "server");
            if (db == null)
                throw new ArgumentException("db cannot be null", "db");

            HttpWebRequest httpWebRequest = null;
            Stream outStream = null;

            // Create the request
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + server.Host + ":" + server.Port.ToString() + "/" +
                        ((db != null) ? db.Name + "/" : "") +
                        ((path != null && path != "") ? path : "") +
                        ((query != null && query != "") ? "?" + query : ""));
            }
            catch (Exception e)
            {
                throw new NetException("Unable to create the HttpWebRequest", e);
            }

            if (httpWebRequest == null)
                throw new NetException("Unable to create the HttpWebRequest");

            // Set headers
            if (operation == OperationType.NULL)
                throw new NetException("Argument operation was set to type null, which is invalid.");

            httpWebRequest.Method = operation.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.KeepAlive = keepAlive;
            httpWebRequest.ServicePoint.Expect100Continue = use100Continue;
            httpWebRequest.ContentLength = contentLength;

            if (!string.IsNullOrEmpty(server.EncodedCredentials))
                httpWebRequest.Headers.Add("Authorization", server.EncodedCredentials);

            if (dataStreamMethod == DataStreamMethod.LoadToMemory)
            {
                httpWebRequest.SendChunked = false;
                httpWebRequest.AllowWriteStreamBuffering = false;
            }
            else // Stream
            {
                if (operation == OperationType.POST || operation == OperationType.PUT)
                {
                    httpWebRequest.SendChunked = true;
                    httpWebRequest.AllowWriteStreamBuffering = true;
                }
                else
                {
                    httpWebRequest.SendChunked = false;
                    httpWebRequest.AllowWriteStreamBuffering = false;
                }
            }

            Logger.Network.Debug("Getting the request stream.");

            try { outStream = httpWebRequest.GetRequestStream(); }
            catch (Exception e)
            {
                throw new NetException("Unable to get the request stream", e);
            }

            Logger.Network.Debug("Request stream acquired.");

            return outStream;
        }

        /// <summary>
        /// Synchronously sends the message.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="db">The db.</param>
        /// <param name="path">The path.</param>
        /// <param name="query">The query.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="dataStreamMethod">The data stream method.</param>
        /// <param name="requestStream">The request stream.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="keepAlive">if set to <c>true</c> [keep alive].</param>
        /// <param name="use100Continue">if set to <c>true</c> [use100 continue].</param>
        /// <param name="useDeflate">if set to <c>true</c> [use deflate].</param>
        /// <param name="useGzip">if set to <c>true</c> [use gzip].</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public ServerResponse SendMessageSync(Server server, Database db, string path, string query, OperationType operation,
            DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, bool keepAlive, bool use100Continue,
            bool useDeflate, bool useGzip, out WebState state)
        {
            if (server == null)
                throw new ArgumentException("server cannot be null", "server");
            if (db == null)
                throw new ArgumentException("db cannot be null", "db");

            HttpWebRequest httpWebRequest = null;
            state = null;
            Stream outStream = null;
            byte[] buffer;
            BinaryReader br;
            BinaryWriter bw;
            int bytesRead = 0;
            long totalBytes = 0, bytesSent = 0;

            // Setup
            state = new WebState();

            // Create the request
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + server.Host + ":" + server.Port.ToString() + "/" +
                        ((db != null) ? db.Name + "/" : "") +
                        ((path != null && path != "") ? path : "") +
                        ((query != null && query != "") ? "?" + query : ""));
            }
            catch (Exception e)
            {
                return new ServerResponse("Unable to create the HttpWebRequest", e.Message, e);
            }

            if (httpWebRequest == null)
                return new ServerResponse("Unable to create the HttpWebRequest", "Unknown");

            // Set headers
            if (operation == OperationType.NULL)
                return new ServerResponse("Invalid operation", "Argument operation was set to type null, which is invalid."); 
            
            httpWebRequest.Method = operation.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.KeepAlive = keepAlive;
            httpWebRequest.ServicePoint.Expect100Continue = use100Continue;

            if (requestStream != null && requestStream.CanSeek)
                httpWebRequest.ContentLength = requestStream.Length;

            if (!string.IsNullOrEmpty(server.EncodedCredentials))
                httpWebRequest.Headers.Add("Authorization", server.EncodedCredentials);

            if (dataStreamMethod == DataStreamMethod.LoadToMemory)
            {
                if (requestStream != null && !requestStream.CanSeek)
                    return new ServerResponse("Unable to seek within the requestStream", "The requestStream must be able to be seekable when DataStreamMethod.LoadToMemory is used.");

                httpWebRequest.SendChunked = false;
                httpWebRequest.AllowWriteStreamBuffering = false;
            }
            else // Stream
            {
                if (operation == OperationType.POST || operation == OperationType.PUT)
                {
                    httpWebRequest.SendChunked = true;
                    httpWebRequest.AllowWriteStreamBuffering = true;
                }
                else
                {
                    httpWebRequest.SendChunked = false;
                    httpWebRequest.AllowWriteStreamBuffering = false;
                }
            }

            // Set requestState
            state.BufferSize = server.BufferSize;
            state.DataStreamMethod = dataStreamMethod;
            state.Operation = operation;
            state.Request = httpWebRequest;
            state.Response = null;
            state.TimeoutDuration = server.Timeout;
            state.Stream = requestStream;

            if (contentType == "application/json")
                state.StreamIsBinary = false;
            else
                state.StreamIsBinary = true;

            if (OnBeforeHeadersAreLocked != null) OnBeforeHeadersAreLocked(state);

            if (state.Stream != null)
            {
                try { outStream = httpWebRequest.GetRequestStream(); }
                catch (Exception e)
                {
                    throw new NetException("Unable to get the request stream", e);
                }

                if (!state.Stream.CanRead)
                    throw new NetException("The request stream cannot be read");

                // As a precaution, reset the stream's position
                if (state.Stream.CanSeek)
                    state.Stream.Position = 0;

                if (state.DataStreamMethod == DataStreamMethod.LoadToMemory)
                {
                    buffer = new byte[state.Stream.Length];
                    if (state.StreamIsBinary)
                    {
                        bw = new BinaryWriter(outStream);
                        br = new BinaryReader(state.Stream);
                        bytesRead = br.Read(buffer, 0, buffer.Length);
                        bw.Write(buffer, 0, bytesRead);
                        bw.Close();
                        br.Close();
                    }
                    else
                    {
                        bytesRead = state.Stream.Read(buffer, 0, buffer.Length);
                        outStream.Write(buffer, 0, bytesRead);
                        outStream.Close();
                        state.Stream.Close();
                    }

                    if (OnUploadProgress != null) OnUploadProgress(state, 1, buffer.Length, buffer.Length);
                }
                else // Stream
                {
                    buffer = new byte[state.BufferSize];
                    totalBytes = state.Stream.Length;
                    if (state.StreamIsBinary)
                    {
                        bw = new BinaryWriter(outStream);
                        br = new BinaryReader(state.Stream);
                        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, bytesRead);
                            bytesSent += bytesRead;
                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                        }
                        bw.Close();
                        br.Close();
                    }
                    else
                    {
                        while ((bytesRead = state.Stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, bytesRead);
                            bytesSent += bytesRead;
                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                        }
                        outStream.Close();
                        state.Stream.Close();
                    }
                }
            }

            // RESPONSE WORK
            try
            {
                state.Response = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception e)
            {
                throw new NetException("Unable to get the server response", e);
            }

            // Get the response's stream
            try
            {
                state.Stream = state.Response.GetResponseStream();
            }
            catch (Exception e)
            {
                throw new NetException("Unable to get the response stream", e);
            }

            return new ServerResponse(true);
        }


        /// <summary>
        /// <see cref="ManualResetEvent"/>
        /// </summary>
        public ManualResetEvent AllDone = new ManualResetEvent(false);

        /// <summary>
        /// Asynchronously sends the message
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="db">The db.</param>
        /// <param name="path">The path.</param>
        /// <param name="query">The query.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="dataStreamMethod">The data stream method.</param>
        /// <param name="requestStream">The request stream.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="keepAlive">if set to <c>true</c> [keep alive].</param>
        /// <param name="use100Continue">if set to <c>true</c> [use100 continue].</param>
        /// <param name="useDeflate">if set to <c>true</c> [use deflate].</param>
        /// <param name="useGzip">if set to <c>true</c> [use gzip].</param>
        /// <returns></returns>
        public ServerResponse SendMessage(Server server, Database db, string path, string query, OperationType operation,
            DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, bool keepAlive, bool use100Continue,
            bool useDeflate, bool useGzip)
        {
            if (server == null)
                throw new ArgumentException("server cannot be null", "server");
            if (db == null)
                throw new ArgumentException("db cannot be null", "db");

            HttpWebRequest httpWebRequest = null;
            IAsyncResult asyncResult = null;
            WebState state = null;

            // Setup
            state = new WebState();

            // Create the request
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + server.Host + ":" + server.Port.ToString() + "/" +
                        ((db != null) ? db.Name + "/" : "") + 
                        ((path != null && path != "") ? path : "") +
                        ((query != null && query != "") ? "?" + query : ""));
            }
            catch (Exception e)
            {
                return new ServerResponse("Unable to create the HttpWebRequest", e.Message, e);
            }

            if (httpWebRequest == null)
                return new ServerResponse("Unable to create the HttpWebRequest", "Unknown");

            // Set headers
            if (operation == OperationType.NULL)
                return new ServerResponse("Invalid operation", "Argument operation was set to type null, which is invalid.");

            httpWebRequest.Method = operation.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.KeepAlive = keepAlive;
            httpWebRequest.ServicePoint.Expect100Continue = use100Continue;
            
            if(requestStream != null && requestStream.CanSeek)
                httpWebRequest.ContentLength = requestStream.Length;

            if (!string.IsNullOrEmpty(server.EncodedCredentials))
                httpWebRequest.Headers.Add("Authorization", server.EncodedCredentials);

            if (dataStreamMethod == DataStreamMethod.LoadToMemory)
            {
                if (requestStream != null && !requestStream.CanSeek)
                    return new ServerResponse("Unable to seek within the requestStream", "The requestStream must be able to be seekable when DataStreamMethod.LoadToMemory is used.");

                httpWebRequest.SendChunked = false;
                httpWebRequest.AllowWriteStreamBuffering = false;
            }
            else // Stream
            {
                if (operation == OperationType.POST || operation == OperationType.PUT)
                {
                    httpWebRequest.SendChunked = true;
                    httpWebRequest.AllowWriteStreamBuffering = true;
                }
                else
                {
                    httpWebRequest.SendChunked = false;
                    httpWebRequest.AllowWriteStreamBuffering = false;
                }
            }

            // Set requestState
            state.BufferSize = server.BufferSize;
            state.DataStreamMethod = dataStreamMethod;
            state.Operation = operation;
            state.Request = httpWebRequest;
            state.Response = null;
            state.TimeoutDuration = server.Timeout;
            state.Stream = requestStream;

            if (contentType == "application/json")
                state.StreamIsBinary = false;
            else
                state.StreamIsBinary = true;

            if (OnBeforeHeadersAreLocked != null) OnBeforeHeadersAreLocked(state);

            if (state.Stream != null)
            { // Is there a stream to send?
                // Get the request stream
                if (OnBeforeBeginGetRequestStream != null) OnBeforeBeginGetRequestStream(state);
                asyncResult = httpWebRequest.BeginGetRequestStream(new AsyncCallback(MessageRequestCallback), state);
            }
            else
            {
                // Get the response stream
                if (OnBeforeBeginGetResponse != null) OnBeforeBeginGetResponse(state);
                asyncResult = httpWebRequest.BeginGetResponse(new AsyncCallback(MessageResponseCallback), state);
            }

            // Register our custom timeout callback
            state.TimeoutWaitHandle = asyncResult.AsyncWaitHandle;
            state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.TimeoutWaitHandle,
                new WaitOrTimerCallback(MessageTimeoutCallback), state, state.TimeoutDuration, true);

            return new ServerResponse(true);
        }

        /// <summary>
        /// Called when a request stream can be used to read data from the stream.
        /// </summary>
        /// <param name="result">The result.</param>
        private void MessageRequestCallback(IAsyncResult result)
        {
            WebState state;
            BinaryReader br = null;
            BinaryWriter bw = null;
            byte[] buffer;
            int bytesRead;
            Stream s;
            long totalBytes, bytesSent = 0;
            
            // Setup
            state = (WebState)result.AsyncState;

            // If the request has timed-out, do nothing
            if (state.IsTimeout) return;

            // Stop the timeout
            if (state.TimeoutWaitHandle == null)
                Thread.Sleep(250);
            if (state.TimeoutWaitHandle != null)
                state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

            // Make sure we have a useable stream
            if (state.Stream == null)
                throw new NetException("The request stream was null");
            if (!state.Stream.CanRead)
                throw new NetException("The request stream cannot be read");

            // Grab the request stream and wrap it with a binary writer
            try
            {
                s = (Stream)state.Request.EndGetRequestStream(result);
            }
            catch (Exception e)
            {
                throw new NetException("Unable to get the request stream", e);
            }

            if (OnAfterEndGetRequestStream != null) OnAfterEndGetRequestStream(state);

            // As a precaution, reset the stream's position
            if (state.Stream.CanSeek)
                state.Stream.Position = 0;

            if (state.DataStreamMethod == DataStreamMethod.LoadToMemory)
            {
                buffer = new byte[state.Stream.Length];
                if (state.StreamIsBinary)
                {
                    bw = new BinaryWriter(s);
                    br = new BinaryReader(state.Stream);
                    bytesRead = br.Read(buffer, 0, buffer.Length);
                    bw.Write(buffer, 0, bytesRead);
                    bw.Close();
                    br.Close();
                }
                else
                {
                    bytesRead = state.Stream.Read(buffer, 0, buffer.Length);
                    s.Write(buffer, 0, bytesRead);
                    s.Close();
                    state.Stream.Close();
                }

                if (OnUploadProgress != null) OnUploadProgress(state, 1, buffer.Length, buffer.Length);
            }
            else // Stream
            {
                buffer = new byte[state.BufferSize];
                totalBytes = state.Stream.Length;
                if (state.StreamIsBinary)
                {
                    bw = new BinaryWriter(s);
                    br = new BinaryReader(state.Stream);
                    while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, bytesRead);
                        bytesSent += bytesRead;
                        if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                    }
                    bw.Close();
                    br.Close();
                }
                else
                {
                    while ((bytesRead = state.Stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        s.Write(buffer, 0, bytesRead);
                        bytesSent += bytesRead;
                        if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                    }
                    s.Close();
                    state.Stream.Close();
                }
            }


            if (OnBeforeBeginGetResponse != null) OnBeforeBeginGetResponse(state);

            result = state.Request.BeginGetResponse(new AsyncCallback(MessageResponseCallback), state);

            // Register our custom timeout callback
            state.TimeoutWaitHandle = result.AsyncWaitHandle;
            state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.TimeoutWaitHandle,
                new WaitOrTimerCallback(MessageTimeoutCallback), state, state.TimeoutDuration, true);
        }

        /// <summary>
        /// Called when a response is returned to access its stream.
        /// </summary>
        /// <param name="result">The result.</param>
        private void MessageResponseCallback(IAsyncResult result)
        {
            WebState state = null;

            // Setup
            state = (WebState)result.AsyncState;

            // If the request has timed-out, do nothing
            if (state.IsTimeout) return;

            // Stop the timeout
            state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

            // If the request has timed-out, do nothing
            if (state.IsTimeout) return;
            try
            {
                state.Response = (HttpWebResponse)state.Request.EndGetResponse(result);
            }
            catch (Exception e)
            {
                throw new NetException("Unable to get the server response", e);
            }

            if (OnAfterEndGetResponse != null) OnAfterEndGetResponse(state);

            // Get the response's stream
            try
            {
                state.Stream = state.Response.GetResponseStream();
            }
            catch (Exception e)
            {
                throw new NetException("Unable to get the response stream", e);
            }

            if (OnComplete != null) OnComplete(state);

            // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
            AllDone.Set();
        }

        /// <summary>
        /// Called when a timeout event occurrs.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="timedOut">if set to <c>true</c> [timed out].</param>
        private void MessageTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                lock (state)
                {
                    WebState webstate = (WebState)state;
                    if (webstate != null)
                    {
                        // Unregister the handle
                        webstate.TimeoutRegisteredWaitHandle.Unregister(webstate.TimeoutWaitHandle);

                        // Update the state
                        webstate.IsTimeout = true;

                        if (webstate.Request != null)
                        {
                            // Abort the HttpWebRequest
                            webstate.Request.Abort();
                        }

                        if (OnTimeout != null) OnTimeout(webstate);

                        // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
                        AllDone.Set();
                    }
                }
            }
        }
    }
}