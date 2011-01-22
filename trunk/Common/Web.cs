using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Common
{
    //public class Web
    //{
    //    public enum OperationType
    //    {
    //        NULL,
    //        GET,
    //        PUT,
    //        POST,
    //        DELETE,
    //        COPY,
    //        HEAD
    //    }

    //    public enum DataStreamMethod
    //    {
    //        Memory,
    //        Stream
    //    }

    //    public class WebState : IDisposable
    //    {
    //        public HttpWebRequest Request;
    //        public HttpWebResponse Response;
    //        public DataStreamMethod DataStreamMethod;
    //        public OperationType OperationType;
    //        public Guid Guid;
    //        public Common.Data.AssetType AssetType;
    //        public int BufferSize;
    //        public int TimeoutDuration;
    //        /// <summary>
    //        /// If OperationType is GET/HEAD this is the Stream associated with the Response (OUT)
    //        /// If OperationType is PUT/POST this is the Stream associated with the Request (IN)
    //        /// </summary>
    //        public Stream Stream;
    //        public bool StreamIsBinary;
    //        public WaitHandle TimeoutWaitHandle;
    //        public RegisteredWaitHandle TimeoutRegisteredWaitHandle;
    //        public bool IsTimeout;


    //        public void Dispose()
    //        {
    //            Request = null;
    //            if(Response != null) Response.Close();
    //            Response = null;
    //            if (Stream != null)
    //            {
    //                Stream.Close();
    //                Stream.Dispose();
    //            }
    //            if(TimeoutWaitHandle != null) TimeoutWaitHandle.Dispose();
    //            TimeoutRegisteredWaitHandle = null;
    //        }
    //    }

    //    public delegate void MessageHandler(WebState state);
    //    public delegate void MessageProgressHandler(WebState state, decimal percent, Int64 bytesSent, Int64 bytesTotal);

    //    public event MessageHandler OnComplete;
    //    public event MessageHandler OnTimeout;

    //    public event MessageProgressHandler OnUploadProgress;

    //    public ManualResetEvent AllDone = new ManualResetEvent(false);

    //    public WebState SendMessage(string host, int port, string virtualPath, Guid guid, Common.Data.AssetType assetType, OperationType operation,
    //                                DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, long? contentLength, string encodedCredentials,
    //                                bool keepAlive, bool use100Continue, bool useDeflate, bool useGzip, int bufferSize, int timeoutDuration)
    //    {
    //        if (string.IsNullOrEmpty(host))
    //            throw new ArgumentException("host cannot be null", "host");
    //        if (port <= 0 || port > int.MaxValue)
    //            throw new ArgumentException("post must be between 0 and " + int.MaxValue, "port");
    //        if (operation == OperationType.NULL)
    //            throw new ArgumentException("operation cannot be set to null", "operation");

    //        HttpWebRequest httpWebRequest = null;
    //        Stream outStream = null;
    //        Web.WebState state;
    //        byte[] buffer;
    //        BinaryReader br;
    //        BinaryWriter bw;
    //        int bytesRead = 0;
    //        long totalBytes = 0, bytesSent = 0;

    //        try
    //        {
    //            string uri = "http://" + host + ":" + port.ToString() + "/";
    //            if (!string.IsNullOrEmpty(virtualPath))
    //                uri += virtualPath + "/";
    //            if ((guid != Guid.Empty) && !Common.Data.AssetType.IsNullOrUnknown(assetType))
    //                uri += guid.ToString("N") + assetType.ToString();
    //            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to create the HttpWebRequest", e);
    //        }

    //        if (httpWebRequest == null)
    //            throw new WebException("Unable to create the HttpWebRequest");

    //        httpWebRequest.Method = operation.ToString();
    //        httpWebRequest.ContentType = contentType;
    //        httpWebRequest.KeepAlive = keepAlive;
    //        httpWebRequest.ServicePoint.Expect100Continue = use100Continue;
    //        if (contentLength.HasValue)
    //            httpWebRequest.ContentLength = contentLength.Value;

    //        if (requestStream != null && requestStream.CanSeek)
    //            httpWebRequest.ContentLength = requestStream.Length;

    //        if (!string.IsNullOrEmpty(encodedCredentials))
    //            httpWebRequest.Headers.Add("Authorization", encodedCredentials);

    //        state = new WebState();
    //        state.Guid = guid;
    //        state.AssetType = assetType;
    //        state.BufferSize = bufferSize;
    //        state.DataStreamMethod = dataStreamMethod;
    //        state.OperationType = operation;
    //        state.Request = httpWebRequest;
    //        state.Response = null;
    //        state.TimeoutDuration = timeoutDuration;

    //        if (contentType == "text/xml")
    //            state.StreamIsBinary = false;
    //        else
    //            state.StreamIsBinary = true;

    //        if (dataStreamMethod == DataStreamMethod.Memory)
    //        {
    //            if (requestStream != null && !requestStream.CanSeek)
    //                throw new IOException("Unable to seek within the requestStream");

    //            httpWebRequest.SendChunked = false;
    //            httpWebRequest.AllowWriteStreamBuffering = false;
    //        }
    //        else // Stream
    //        {
    //            if (operation == OperationType.POST || operation == OperationType.PUT)
    //            {
    //                httpWebRequest.SendChunked = true;
    //                httpWebRequest.AllowWriteStreamBuffering = true;

    //                // Make sure we have a useable stream passed through the argument
    //                if (requestStream == null)
    //                    throw new IOException("The request stream was null");
    //                if (!requestStream.CanRead)
    //                    throw new IOException("The request stream cannot be read");

    //                // Grab the request stream
    //                try { outStream = httpWebRequest.GetRequestStream(); }
    //                catch (Exception e)
    //                {
    //                    throw new WebException("Unable to get the request stream", e);
    //                }

    //                // As a precaution, reset the stream's position
    //                if (requestStream.CanSeek)
    //                    requestStream.Position = 0;

    //                if (dataStreamMethod == DataStreamMethod.Memory)
    //                {
    //                    buffer = new byte[requestStream.Length];
    //                    if (state.StreamIsBinary)
    //                    {
    //                        bw = new BinaryWriter(outStream);
    //                        br = new BinaryReader(requestStream);
    //                        bytesRead = br.Read(buffer, 0, buffer.Length);
    //                        bw.Write(buffer, 0, bytesRead);
    //                        bw.Close();
    //                        br.Close();
    //                    }
    //                    else
    //                    {
    //                        bytesRead = requestStream.Read(buffer, 0, buffer.Length);
    //                        outStream.Write(buffer, 0, bytesRead);
    //                        outStream.Close();
    //                        requestStream.Close();
    //                    }

    //                    if (OnUploadProgress != null) OnUploadProgress(state, 1, buffer.Length, buffer.Length);
    //                }
    //                else // Stream
    //                {
    //                    buffer = new byte[state.BufferSize];
    //                    totalBytes = requestStream.Length;
    //                    if (state.StreamIsBinary)
    //                    {
    //                        bw = new BinaryWriter(outStream);
    //                        br = new BinaryReader(requestStream);
    //                        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
    //                        {
    //                            bw.Write(buffer, 0, bytesRead);
    //                            bytesSent += bytesRead;
    //                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                        }
    //                        bw.Close();
    //                        br.Close();
    //                    }
    //                    else
    //                    {
    //                        while ((bytesRead = requestStream.Read(buffer, 0, buffer.Length)) > 0)
    //                        {
    //                            outStream.Write(buffer, 0, bytesRead);
    //                            bytesSent += bytesRead;
    //                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                        }
    //                        outStream.Close();
    //                        requestStream.Close();
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                httpWebRequest.SendChunked = false;
    //                httpWebRequest.AllowWriteStreamBuffering = false;
    //            }
    //        }


    //        // RESPONSE WORK
    //        try
    //        {
    //            state.Response = (HttpWebResponse)httpWebRequest.GetResponse();
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the server response", e);
    //        }

    //        // Get the response's stream
    //        try
    //        {
    //            state.Stream = state.Response.GetResponseStream();
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the response stream", e);
    //        }

    //        return state;
    //    }

    //    public WebState SendMessage(string host, int port, string virtualPath, string filename, OperationType operation,
    //                                DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, long? contentLength, string encodedCredentials,
    //                                bool keepAlive, bool use100Continue, bool useDeflate, bool useGzip, int bufferSize, int timeoutDuration)
    //    {
    //        if (string.IsNullOrEmpty(host))
    //            throw new ArgumentException("host cannot be null", "host");
    //        if (port <= 0 || port > int.MaxValue)
    //            throw new ArgumentException("post must be between 0 and " + int.MaxValue, "port");
    //        if (operation == OperationType.NULL)
    //            throw new ArgumentException("operation cannot be set to null", "operation");

    //        HttpWebRequest httpWebRequest = null;
    //        Stream outStream = null;
    //        Web.WebState state;
    //        byte[] buffer;
    //        BinaryReader br;
    //        BinaryWriter bw;
    //        int bytesRead = 0;
    //        long totalBytes = 0, bytesSent = 0;

    //        // Create the request
    //        try
    //        {
    //            string uri = "http://" + host + ":" + port.ToString() + "/";
    //            if (!string.IsNullOrEmpty(virtualPath))
    //                uri += virtualPath + "/";
    //            if (!string.IsNullOrEmpty(filename))
    //                uri += filename;
    //            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to create the HttpWebRequest", e);
    //        }

    //        if (httpWebRequest == null)
    //            throw new WebException("Unable to create the HttpWebRequest");

    //        httpWebRequest.Method = operation.ToString();
    //        httpWebRequest.ContentType = contentType;
    //        httpWebRequest.KeepAlive = keepAlive;
    //        httpWebRequest.ServicePoint.Expect100Continue = use100Continue;
    //        if(contentLength.HasValue)
    //            httpWebRequest.ContentLength = contentLength.Value;

    //        if (requestStream != null && requestStream.CanSeek)
    //            httpWebRequest.ContentLength = requestStream.Length;

    //        if (!string.IsNullOrEmpty(encodedCredentials))
    //            httpWebRequest.Headers.Add("Authorization", encodedCredentials);
            
    //        state = new WebState();
    //        state.Guid = Guid.Empty;
    //        state.AssetType = null;
    //        state.BufferSize = bufferSize;
    //        state.DataStreamMethod = dataStreamMethod;
    //        state.OperationType = operation;
    //        state.Request = httpWebRequest;
    //        state.Response = null;
    //        state.TimeoutDuration = timeoutDuration;

    //        if (contentType == "text/xml")
    //            state.StreamIsBinary = false;
    //        else
    //            state.StreamIsBinary = true;

    //        if (dataStreamMethod == DataStreamMethod.Memory)
    //        {
    //            if (requestStream != null && !requestStream.CanSeek)
    //                throw new IOException("Unable to seek within the requestStream");

    //            httpWebRequest.SendChunked = false;
    //            httpWebRequest.AllowWriteStreamBuffering = false;
    //        }
    //        else // Stream
    //        {
    //            if (operation == OperationType.POST || operation == OperationType.PUT)
    //            {
    //                httpWebRequest.SendChunked = true;
    //                httpWebRequest.AllowWriteStreamBuffering = true; 
                    
    //                // Make sure we have a useable stream passed through the argument
    //                if (requestStream == null)
    //                    throw new IOException("The request stream was null");
    //                if (!requestStream.CanRead)
    //                    throw new IOException("The request stream cannot be read");

    //                // Grab the request stream
    //                try { outStream = httpWebRequest.GetRequestStream(); }
    //                catch (Exception e)
    //                {
    //                    throw new WebException("Unable to get the request stream", e);
    //                }

    //                // As a precaution, reset the stream's position
    //                if (requestStream.CanSeek)
    //                    requestStream.Position = 0;

    //                if (dataStreamMethod == DataStreamMethod.Memory)
    //                {
    //                    buffer = new byte[requestStream.Length];
    //                    if (state.StreamIsBinary)
    //                    {
    //                        bw = new BinaryWriter(outStream);
    //                        br = new BinaryReader(requestStream);
    //                        bytesRead = br.Read(buffer, 0, buffer.Length);
    //                        bw.Write(buffer, 0, bytesRead);
    //                        bw.Close();
    //                        br.Close();
    //                    }
    //                    else
    //                    {
    //                        bytesRead = requestStream.Read(buffer, 0, buffer.Length);
    //                        outStream.Write(buffer, 0, bytesRead);
    //                        outStream.Close();
    //                        requestStream.Close();
    //                    }

    //                    if (OnUploadProgress != null) OnUploadProgress(state, 1, buffer.Length, buffer.Length);
    //                }
    //                else // Stream
    //                {
    //                    buffer = new byte[state.BufferSize];
    //                    totalBytes = requestStream.Length;
    //                    if (state.StreamIsBinary)
    //                    {
    //                        bw = new BinaryWriter(outStream);
    //                        br = new BinaryReader(requestStream);
    //                        while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
    //                        {
    //                            bw.Write(buffer, 0, bytesRead);
    //                            bytesSent += bytesRead;
    //                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                        }
    //                        bw.Close();
    //                        br.Close();
    //                    }
    //                    else
    //                    {
    //                        while ((bytesRead = requestStream.Read(buffer, 0, buffer.Length)) > 0)
    //                        {
    //                            outStream.Write(buffer, 0, bytesRead);
    //                            bytesSent += bytesRead;
    //                            if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                        }
    //                        outStream.Close();
    //                        requestStream.Close();
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                httpWebRequest.SendChunked = false;
    //                httpWebRequest.AllowWriteStreamBuffering = false;
    //            }
    //        }
            

    //        // RESPONSE WORK
    //        try
    //        {
    //            state.Response = (HttpWebResponse)httpWebRequest.GetResponse();
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the server response", e);
    //        }

    //        // Get the response's stream
    //        try
    //        {
    //            state.Stream = state.Response.GetResponseStream();
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the response stream", e);
    //        }

    //        return state;
    //    }

    //    public Stream GetRequestStreamForMessage(string host, int port, string virtualPath, Guid guid, Common.Data.AssetType assetType, OperationType operation,
    //        DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, long contentLength, string encodedCredentials,
    //        bool keepAlive, bool use100Continue, bool useDeflate, bool useGzip, int bufferSize, int timeoutDuration)
    //    {
    //        if (string.IsNullOrEmpty(host))
    //            throw new ArgumentException("host cannot be null", "host");
    //        if (port <= 0 || port > int.MaxValue)
    //            throw new ArgumentException("post must be between 0 and " + int.MaxValue, "port");
    //        if (operation == OperationType.NULL)
    //            throw new ArgumentException("operation cannot be set to null", "operation");

    //        HttpWebRequest httpWebRequest = null;
    //        Stream outStream = null;
    //        Web.WebState state;

    //        // Create the request
    //        try
    //        {
    //            string uri = "http://" + host + ":" + port.ToString() + "/";
    //            if (!string.IsNullOrEmpty(virtualPath))
    //                uri += virtualPath + "/";
    //            if ((guid != Guid.Empty) && !Common.Data.AssetType.IsNullOrUnknown(assetType))
    //                uri += guid.ToString("N") + assetType.ToString();
    //            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to create the HttpWebRequest", e);
    //        }

    //        if (httpWebRequest == null)
    //            throw new WebException("Unable to create the HttpWebRequest");
            
    //        httpWebRequest.Method = operation.ToString();
    //        httpWebRequest.ContentType = contentType;
    //        httpWebRequest.KeepAlive = keepAlive;
    //        httpWebRequest.ServicePoint.Expect100Continue = use100Continue;
    //        httpWebRequest.ContentLength = contentLength;

    //        if (requestStream != null && requestStream.CanSeek)
    //            httpWebRequest.ContentLength = requestStream.Length;

    //        if (!string.IsNullOrEmpty(encodedCredentials))
    //            httpWebRequest.Headers.Add("Authorization", encodedCredentials);

    //        if (dataStreamMethod == DataStreamMethod.Memory)
    //        {
    //            if (requestStream != null && !requestStream.CanSeek)
    //                throw new IOException("Unable to seek within the requestStream");

    //            httpWebRequest.SendChunked = false;
    //            httpWebRequest.AllowWriteStreamBuffering = false;
    //        }
    //        else // Stream
    //        {
    //            if (operation == OperationType.POST || operation == OperationType.PUT)
    //            {
    //                httpWebRequest.SendChunked = true;
    //                httpWebRequest.AllowWriteStreamBuffering = true;
    //            }
    //            else
    //            {
    //                httpWebRequest.SendChunked = false;
    //                httpWebRequest.AllowWriteStreamBuffering = false;
    //            }
    //        }

    //        try { outStream = httpWebRequest.GetRequestStream(); }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the request stream", e);
    //        }
            
    //        state = new WebState();
    //        state.Guid = guid;
    //        state.AssetType = assetType;
    //        state.BufferSize = bufferSize;
    //        state.DataStreamMethod = dataStreamMethod;
    //        state.OperationType = operation;
    //        state.Request = httpWebRequest;
    //        state.Response = null;
    //        state.TimeoutDuration = timeoutDuration;
    //        state.Stream = requestStream;

    //        return outStream;
    //    }

    //    public void SendMessageAsync(string host, int port, string virtualPath, Guid guid, Common.Data.AssetType assetType, OperationType operation,
    //        DataStreamMethod dataStreamMethod, Stream requestStream, string contentType, long contentLength, string encodedCredentials,
    //       bool keepAlive, bool use100Continue, int bufferSize, int timeoutDuration)
    //    {
    //        if (string.IsNullOrEmpty(host))
    //            throw new ArgumentException("host cannot be null", "host");
    //        if (port <= 0 || port > int.MaxValue)
    //            throw new ArgumentException("post must be between 0 and " + int.MaxValue, "port");
    //        if (operation == OperationType.NULL)
    //            throw new ArgumentException("operation cannot be set to null", "operation");

    //        HttpWebRequest httpWebRequest = null;
    //        IAsyncResult asyncResult = null;
    //        WebState state = null;

    //        // Create the request
    //        try
    //        {
    //            string uri = "http://" + host + ":" + port.ToString() + "/";
    //            if (!string.IsNullOrEmpty(virtualPath))
    //                uri += virtualPath + "/";
    //            if ((guid != Guid.Empty) && !Common.Data.AssetType.IsNullOrUnknown(assetType))
    //                uri += guid.ToString("N") + assetType.ToString();
    //            httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to create the HttpWebRequest", e);
    //        }

    //        if (httpWebRequest == null)
    //            throw new WebException("Unable to create the HttpWebRequest");

    //        httpWebRequest.Method = operation.ToString();
    //        httpWebRequest.ContentType = contentType;
    //        httpWebRequest.KeepAlive = keepAlive;
    //        httpWebRequest.ServicePoint.Expect100Continue = use100Continue;

    //        if (requestStream != null && requestStream.CanSeek)
    //            httpWebRequest.ContentLength = requestStream.Length;

    //        if (!string.IsNullOrEmpty(encodedCredentials))
    //            httpWebRequest.Headers.Add("Authorization", encodedCredentials);

    //        if (dataStreamMethod == DataStreamMethod.Memory)
    //        {
    //            if (requestStream != null && !requestStream.CanSeek)
    //                throw new IOException("Unable to seek within the requestStream");

    //            httpWebRequest.SendChunked = false;
    //            httpWebRequest.AllowWriteStreamBuffering = false;
    //        }
    //        else
    //        {
    //            if (operation == OperationType.POST || operation == OperationType.PUT)
    //            {
    //                httpWebRequest.SendChunked = true;
    //                httpWebRequest.AllowWriteStreamBuffering = true;
    //            }
    //            else
    //            {
    //                httpWebRequest.SendChunked = false;
    //                httpWebRequest.AllowWriteStreamBuffering = false;
    //            }
    //        }

    //        state = new WebState();
    //        state.Guid = guid;
    //        state.AssetType = assetType;
    //        state.BufferSize = bufferSize;
    //        state.DataStreamMethod = dataStreamMethod;
    //        state.OperationType = operation;
    //        state.Request = httpWebRequest;
    //        state.Response = null;
    //        state.TimeoutDuration = timeoutDuration;
    //        state.Stream = requestStream;

    //        if (contentType == "text/xml")
    //            state.StreamIsBinary = false;
    //        else
    //            state.StreamIsBinary = true;

    //        if (state.Stream != null)
    //        {
    //            asyncResult = httpWebRequest.BeginGetRequestStream(new AsyncCallback(MessageRequestCallback), state);
    //        }
    //        else
    //        {
    //            asyncResult = httpWebRequest.BeginGetResponse(new AsyncCallback(MessageResponseCallback), state);
    //        }

    //        state.TimeoutWaitHandle = asyncResult.AsyncWaitHandle;
    //        state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.TimeoutWaitHandle,
    //            new WaitOrTimerCallback(MessageTimeoutCallback), state, state.TimeoutDuration, true);
    //    }

    //    private void MessageRequestCallback(IAsyncResult result)
    //    {
    //        WebState state;
    //        BinaryReader br = null;
    //        BinaryWriter bw = null;
    //        byte[] buffer;
    //        int bytesRead;
    //        Stream s;
    //        long totalBytes, bytesSent = 0;
            
    //        // Setup
    //        state = (WebState)result.AsyncState;

    //        // If the request has timed-out, do nothing
    //        if (state.IsTimeout) return;

    //        // Stop the timeout
    //        if (state.TimeoutWaitHandle != null)
    //            state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

    //        // Make sure we have a useable stream
    //        if (state.Stream == null)
    //            throw new Exception("The request stream was null");
    //        if (!state.Stream.CanRead)
    //            throw new Exception("The request stream cannot be read");

    //        // Grab the request stream and wrap it with a binary writer
    //        try
    //        {
    //            s = (Stream)state.Request.EndGetRequestStream(result);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the request stream", e);
    //        }

    //        // As a precaution, reset the stream's position
    //        if (state.Stream.CanSeek)
    //            state.Stream.Position = 0;

    //        if (state.DataStreamMethod == DataStreamMethod.Memory)
    //        {
    //            buffer = new byte[state.Stream.Length];
    //            if (state.StreamIsBinary)
    //            {
    //                bw = new BinaryWriter(s);
    //                br = new BinaryReader(state.Stream);
    //                bytesRead = br.Read(buffer, 0, buffer.Length);
    //                bw.Write(buffer, 0, bytesRead);
    //                bw.Close();
    //                br.Close();
    //            }
    //            else
    //            {
    //                bytesRead = state.Stream.Read(buffer, 0, buffer.Length);
    //                s.Write(buffer, 0, bytesRead);
    //                s.Close();
    //                state.Stream.Close();
    //            }

    //            if (OnUploadProgress != null) OnUploadProgress(state, 1, buffer.Length, buffer.Length);
    //        }
    //        else // Stream
    //        {
    //            buffer = new byte[state.BufferSize];
    //            totalBytes = state.Stream.Length;
    //            if (state.StreamIsBinary)
    //            {
    //                bw = new BinaryWriter(s);
    //                br = new BinaryReader(state.Stream);
    //                while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
    //                {
    //                    bw.Write(buffer, 0, bytesRead);
    //                    bytesSent += bytesRead;
    //                    if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                }
    //                bw.Close();
    //                br.Close();
    //            }
    //            else
    //            {
    //                while ((bytesRead = state.Stream.Read(buffer, 0, buffer.Length)) > 0)
    //                {
    //                    s.Write(buffer, 0, bytesRead);
    //                    bytesSent += bytesRead;
    //                    if (OnUploadProgress != null) OnUploadProgress(state, ((decimal)bytesSent / (decimal)totalBytes), bytesSent, totalBytes);
    //                }
    //                s.Close();
    //                state.Stream.Close();
    //            }
    //        }

    //        result = state.Request.BeginGetResponse(new AsyncCallback(MessageResponseCallback), state);

    //        // Register our custom timeout callback
    //        state.TimeoutWaitHandle = result.AsyncWaitHandle;
    //        state.TimeoutRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(state.TimeoutWaitHandle,
    //            new WaitOrTimerCallback(MessageTimeoutCallback), state, state.TimeoutDuration, true);
    //    }

    //    private void MessageResponseCallback(IAsyncResult result)
    //    {
    //        WebState state = null;

    //        // Setup
    //        state = (WebState)result.AsyncState;

    //        // If the request has timed-out, do nothing
    //        if (state.IsTimeout) return;

    //        // Stop the timeout
    //        state.TimeoutRegisteredWaitHandle.Unregister(state.TimeoutWaitHandle);

    //        // If the request has timed-out, do nothing
    //        if (state.IsTimeout) return;
    //        try
    //        {
    //            state.Response = (HttpWebResponse)state.Request.EndGetResponse(result);
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the server response", e);
    //        }

    //        // Get the response's stream
    //        try
    //        {
    //            state.Stream = state.Response.GetResponseStream();
    //        }
    //        catch (Exception e)
    //        {
    //            throw new WebException("Unable to get the response stream", e);
    //        }

    //        if (OnComplete != null) OnComplete(state);

    //        // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
    //        AllDone.Set();
    //    }

    //    private void MessageTimeoutCallback(object state, bool timedOut)
    //    {
    //        if (timedOut)
    //        {
    //            lock (state)
    //            {
    //                WebState webstate = (WebState)state;
    //                if (webstate != null)
    //                {
    //                    // Unregister the handle
    //                    webstate.TimeoutRegisteredWaitHandle.Unregister(webstate.TimeoutWaitHandle);

    //                    // Update the state
    //                    webstate.IsTimeout = true;

    //                    if (webstate.Request != null)
    //                    {
    //                        // Abort the HttpWebRequest
    //                        webstate.Request.Abort();
    //                    }

    //                    if (OnTimeout != null) OnTimeout(webstate);

    //                    // Set the ManualResetEvent to signaled, allowing other threads that were waiting to continue
    //                    AllDone.Set();
    //                }
    //            }
    //        }
    //    }
    //}
}
