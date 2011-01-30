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

namespace Common.Network
{
    public class Message
    {
        public delegate void MessageHandler(State state);
        public delegate void MessageProgressHandler(State state, decimal percent, Int64 bytesSent, Int64 bytesTotal);

        public event MessageHandler OnComplete;
        public event MessageHandler OnTimeout;

        public event MessageProgressHandler OnUploadProgress;

        public ManualResetEvent AllDone = new ManualResetEvent(false);
        
        private State _state;
        private Guid _guid;
        private Logger _generalLogger;
        private Logger _networkLogger;

        public State State { get { return _state; } }
        public object Tag { get; set; }

        public Message(string host, int port, string virtualPath, Guid guid,
                       Common.Data.AssetType assetType, OperationType operation,
                       DataStreamMethod dataStreamMethod, Stream requestStream,
                       string contentType, long? contentLength, string encodedCredentials,
                       bool keepAlive, bool use100Continue, bool useDeflate, bool useGzip,
                       int bufferSize, int timeoutDuration, Logger generalLogger, Logger networkLogger)
        {
            _guid = guid;
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;

            if (guid == null || guid == Guid.Empty)
            {
                Init(host, port, virtualPath, null, operation, dataStreamMethod, requestStream,
                     contentType, contentLength, encodedCredentials, keepAlive, use100Continue,
                     useDeflate, useGzip, bufferSize, timeoutDuration);
            }
            else// if (Data.AssetType.IsNullOrUnknown(assetType))
            {
                Init(host, port, virtualPath, guid.ToString("N"), operation, dataStreamMethod, 
                     requestStream, contentType, contentLength, encodedCredentials, keepAlive, 
                     use100Continue, useDeflate, useGzip, bufferSize, timeoutDuration);
            }
            //else
            //{
            //    Init(host, port, virtualPath, guid.ToString("N") + assetType.ToString(),
            //         operation, dataStreamMethod, requestStream, contentType, contentLength,
            //         encodedCredentials, keepAlive, use100Continue, useDeflate, useGzip,
            //         bufferSize, timeoutDuration);
            //}
        }

        public Message(string host, int port, string virtualPath, string filename,
                       OperationType operation, DataStreamMethod dataStreamMethod, 
                       Stream requestStream, string contentType, long? contentLength, 
                       string encodedCredentials, bool keepAlive, bool use100Continue,
                       bool useDeflate, bool useGzip, int bufferSize, int timeoutDuration, 
                       Logger generalLogger, Logger networkLogger)
        {
            _generalLogger = generalLogger;
            _networkLogger = networkLogger;

            Init(host, port, virtualPath, filename, operation, dataStreamMethod, requestStream, 
                 contentType, contentLength, encodedCredentials, keepAlive, use100Continue, 
                 useDeflate, useGzip, bufferSize, timeoutDuration);
        }

        public void Init(string host, int port, string virtualPath, string filename,
                         OperationType operation, DataStreamMethod dataStreamMethod, 
                         Stream requestStream, string contentType, long? contentLength, 
                         string encodedCredentials, bool keepAlive, bool use100Continue, 
                         bool useDeflate, bool useGzip, int bufferSize, int timeoutDuration)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host cannot be null", "host");
            if (port <= 0 || port > int.MaxValue)
                throw new ArgumentException("post must be between 0 and " + int.MaxValue, "port");
            if (operation == OperationType.NULL)
                throw new ArgumentException("operation cannot be set to null", "operation");

            HttpWebRequest httpWebRequest = null;

            try
            {
                string uri = "http://" + host + ":" + port.ToString() + "/";
                if (!string.IsNullOrEmpty(virtualPath))
                    uri += virtualPath + "/";
                if (!string.IsNullOrEmpty(filename))
                    uri += filename;
                httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (Exception e)
            {
                if (_generalLogger != null)
                {
                    _generalLogger.Write(Logger.LevelEnum.Normal,
                        "Failed to create the HttpWebRequest object.\r\n" + 
                        Logger.ExceptionToString(e));
                }
                throw new NetworkException(_state, "Unable to create the HttpWebRequest", e);
            }

            if (httpWebRequest == null)
                throw new NetworkException(_state, "Unable to create the HttpWebRequest");


            if (_networkLogger != null)
            {
                _networkLogger.Write(Logger.LevelEnum.Debug, "HttpWebRequest Created.");
            }

            httpWebRequest.Method = operation.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.KeepAlive = keepAlive;
            httpWebRequest.ServicePoint.Expect100Continue = use100Continue;

            // Timeout has no effect on ansyc requests
			httpWebRequest.Timeout = timeoutDuration;
            
            // Set Content Length (auto-sets if it can, allows override)
            if (contentLength.HasValue)
                httpWebRequest.ContentLength = contentLength.Value;
            else if (requestStream != null && requestStream.CanSeek)
                httpWebRequest.ContentLength = requestStream.Length;

            if (!string.IsNullOrEmpty(encodedCredentials))
                httpWebRequest.Headers.Add("Authorization", encodedCredentials);

            _state = new State();
            _state.Guid = _guid;
            _state.BufferSize = bufferSize;
            _state.DataStreamMethod = dataStreamMethod;
            _state.OperationType = operation;
            _state.Request = httpWebRequest;
            _state.TimeoutDuration = timeoutDuration;
            _state.Request.SendChunked = false;
            _state.Request.AllowWriteStreamBuffering = false;
            _state.Stream = requestStream;

            if (contentType != "text/xml")
                _state.StreamIsBinary = true;

            if (dataStreamMethod == DataStreamMethod.Stream && 
                (operation == OperationType.POST || operation == OperationType.PUT))
            {
                // Stream && either post or put (sending data basically)
                //httpWebRequest.SendChunked = true;
                //httpWebRequest.AllowWriteStreamBuffering = true;
            }

            if (_networkLogger != null)
            {
                _networkLogger.Write(Logger.LevelEnum.Debug, "Network Message Initialized:\r\n" +
                    _state.GetLogString());
            }
        }

        public Stream GetNetworkStreamOut()
        {
            Stream outStream = null;

            if (_state.DataStreamMethod == DataStreamMethod.Stream &&
                (_state.OperationType == OperationType.POST || _state.OperationType == OperationType.PUT))
            {
                // Make sure we have a useable stream passed through the argument
                if (_state.Stream == null)
                    throw new IOException("The request stream is null");
                if (!_state.Stream.CanRead)
                    throw new IOException("The request stream cannot be read");
            }

            // Get the network stream to write on
            try { outStream = _state.Request.GetRequestStream(); }
            catch (Exception e)
            {
                if(_networkLogger != null)
                    _networkLogger.Write(Logger.LevelEnum.Normal,
                        "An exception occurred while calling GetRequestStream()\r\n" +
                        Logger.ExceptionToString(e));

                throw new NetworkException(_state, "Unable to get the request stream", e);
            }

            if (_networkLogger != null)
                _networkLogger.Write(Logger.LevelEnum.Debug, 
                    "GetNetorkStreamOut() successfully obtained the request stream.");

            return outStream;
        }

        private Stream GetNetworkStreamIn()
        {
            try
            {
                _state.Response = (HttpWebResponse)_state.Request.GetResponse();
            }
            catch (Exception e)
            {
                if (_networkLogger != null)
                    _networkLogger.Write(Logger.LevelEnum.Normal,
                        "An exception occurred while calling GetResponse()\r\n" +
                        Logger.ExceptionToString(e));

                throw new NetworkException(_state, "Unable to get the server response", e);
            }

            if (_networkLogger != null)
                _networkLogger.Write(Logger.LevelEnum.Debug,
                    "GetNetworkStreamIn() successfully obtained the HttpWebResponse.");

            try
            {
                _state.Stream = _state.Response.GetResponseStream();
            }
			catch(WebException e)
			{
				if(e.Status == WebExceptionStatus.Timeout)
                    if (OnTimeout != null) OnTimeout(_state);
                else
                {
                    if (_networkLogger != null)
                        _networkLogger.Write(Logger.LevelEnum.Normal,
                            "An exception occurred while calling GetResponseStream()\r\n" +
                            Logger.ExceptionToString(e));

                    throw new NetworkException(_state, "Unable to get the response stream", e);
                }
			}
            catch (Exception e)
            {
                if (_networkLogger != null)
                    _networkLogger.Write(Logger.LevelEnum.Normal,
                        "An exception occurred while calling GetResponseStream()\r\n" +
                        Logger.ExceptionToString(e));

                throw new NetworkException(_state, "Unable to get the response stream", e);
            }

            if (_networkLogger != null)
                _networkLogger.Write(Logger.LevelEnum.Debug,
                    "GetNetworkStreamIn() successfully obtained the response stream.");

            return _state.Stream;
        }

        public void Send()
        {
            Stream outStream = null;
            byte[] buffer;
            BinaryReader br;
            BinaryWriter bw;
            int bytesRead = 0;
            long totalBytes = 0, bytesSent = 0;

            // Gets the network stream to write on
            if (_state.OperationType == OperationType.POST || _state.OperationType == OperationType.PUT)
            {
                outStream = GetNetworkStreamOut();

                // As a precaution, reset the stream's position
                if (_state.Stream.CanSeek)
                    _state.Stream.Position = 0;

                if (_state.DataStreamMethod == DataStreamMethod.Memory)
                {
                    buffer = new byte[_state.Stream.Length];

                    if (_networkLogger != null)
                        _networkLogger.Write(Logger.LevelEnum.Debug, 
                            "Starting writing the package to the network stream as one block.");

                    if (_state.StreamIsBinary)
                    {
                        bw = new BinaryWriter(outStream);
                        br = new BinaryReader(_state.Stream);
                        bytesRead = br.Read(buffer, 0, buffer.Length);
                        bw.Write(buffer, 0, bytesRead);
                        bw.Close();
                        br.Close();
                    }
                    else
                    {
                        bytesRead = _state.Stream.Read(buffer, 0, buffer.Length);
                        outStream.Write(buffer, 0, bytesRead);
                        outStream.Close();
                        _state.Stream.Close();
                    }

                    if (_networkLogger != null)
                        _networkLogger.Write(Logger.LevelEnum.Debug,
                            "Finished writing the package to the network stream as one block.");

                    if (OnUploadProgress != null) OnUploadProgress(_state, 1, buffer.Length, buffer.Length);
                }
                else // Stream
                {
                    buffer = new byte[_state.BufferSize];
                    totalBytes = _state.Stream.Length;

                    if (_networkLogger != null)
                        _networkLogger.Write(Logger.LevelEnum.Debug,
                            "Starting writing the package to the network stream in multiple blocks.");

                    if (_state.StreamIsBinary)
                    {
                        bw = new BinaryWriter(outStream);
                        br = new BinaryReader(_state.Stream);
                        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, bytesRead);
                            bytesSent += bytesRead;
                            if (OnUploadProgress != null) OnUploadProgress(_state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                        }
                        bw.Close();
                        br.Close();
                    }
                    else
                    {
                        while ((bytesRead = _state.Stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, bytesRead);
                            bytesSent += bytesRead;
                            if (OnUploadProgress != null) OnUploadProgress(_state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
                        }
                        outStream.Close();
                        _state.Stream.Close();
                    }

                    if (_networkLogger != null)
                        _networkLogger.Write(Logger.LevelEnum.Debug,
                            "Finished writing the package to the network stream in multiple blocks.");
                }

                _state.Stream.Dispose();
            }

            GetNetworkStreamIn();
        }

        public State SendAsync()
        {
            throw new NotSupportedException("Logging has not been implemented on Asynchronous Message Transfer, thus it should not be used.");

            IAsyncResult asyncResult = null;

            if ((_state.OperationType == OperationType.POST || _state.OperationType == OperationType.PUT) &&
                (_state.Stream != null))
            {
                asyncResult = _state.Request.BeginGetRequestStream(new AsyncCallback(MessageRequestCallback), _state);
            }
            else
            {
                asyncResult = _state.Request.BeginGetResponse(new AsyncCallback(MessageResponseCallback), _state);
            }                
            
            _state.TimeoutWaitHandle = asyncResult.AsyncWaitHandle;
            _state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_state.TimeoutWaitHandle,
                new WaitOrTimerCallback(MessageTimeoutCallback), _state, _state.TimeoutDuration, true);

            return _state;
        }

        private void MessageRequestCallback(IAsyncResult result)
        {
            State state;
            BinaryReader br = null;
            BinaryWriter bw = null;
            byte[] buffer;
            int bytesRead;
            Stream outStream;
            long totalBytes, bytesSent = 0;

            // Setup
            state = (State)result.AsyncState;

            // If the request has timed-out, do nothing
            if (state.IsTimeout) return;

            // Stop the timeout
            if (state.TimeoutWaitHandle != null)
                state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

            // Make sure we have a useable stream
            if (state.Stream == null)
                throw new Exception("The request stream was null");
            if (!state.Stream.CanRead)
                throw new Exception("The request stream cannot be read");

            // Grab the request stream and wrap it with a binary writer
            try
            {
                outStream = (Stream)state.Request.EndGetRequestStream(result);
            }
            catch (Exception e)
            {
                throw new WebException("Unable to get the request stream", e);
            }

            // As a precaution, reset the stream's position
            if (state.Stream.CanSeek)
                state.Stream.Position = 0;

            if (state.DataStreamMethod == DataStreamMethod.Memory)
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

            // Clean up the stream so we can reuse it
            state.Stream.Dispose();

            result = state.Request.BeginGetResponse(new AsyncCallback(MessageResponseCallback), state);

            // Register our custom timeout callback
            state.TimeoutWaitHandle = result.AsyncWaitHandle;
            state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.TimeoutWaitHandle,
                new WaitOrTimerCallback(MessageTimeoutCallback), state, state.TimeoutDuration, true);
        }

        private void MessageResponseCallback(IAsyncResult result)
        {
            State state = null;

            // Setup
            state = (State)result.AsyncState;

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
                throw new WebException("Unable to get the server response", e);
            }

            // Get the response's stream
            try
            {
                state.Stream = state.Response.GetResponseStream();
            }
            catch (Exception e)
            {
                throw new WebException("Unable to get the response stream", e);
            }

            if (OnComplete != null) OnComplete(state);

            // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
            AllDone.Set();
        }

        private void MessageTimeoutCallback(object asyncState, bool timedOut)
        {
            if (timedOut)
            {
                lock (asyncState)
                {
                    State state = (State)asyncState;
                    if (state != null)
                    {
                        // Unregister the handle
                        state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

                        // Update the state
                        state.IsTimeout = true;

                        if (state.Request != null)
                        {
                            // Abort the HttpWebRequest
                            state.Request.Abort();
                        }

                        if (OnTimeout != null) OnTimeout(state);

                        // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
                        AllDone.Set();
                    }
                }
            }
        }
    }
}
