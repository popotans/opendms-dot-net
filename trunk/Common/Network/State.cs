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
    /// <summary>
    /// An enumeration of support HTTP operations.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Not set
        /// </summary>
        NULL,
        /// <summary>
        /// HTTP GET
        /// </summary>
        GET,
        /// <summary>
        /// HTTP PUT
        /// </summary>
        PUT,
        /// <summary>
        /// HTTP POST
        /// </summary>
        POST,
        /// <summary>
        /// HTTP DELETE
        /// </summary>
        DELETE,
        /// <summary>
        /// HTTP COPY
        /// </summary>
        COPY,
        /// <summary>
        /// HTTP HEAD
        /// </summary>
        HEAD
    }

    /// <summary>
    /// An enumeration of methods of accessing data.
    /// </summary>
    public enum DataStreamMethod
    {
        /// <summary>
        /// All data is saved into memory
        /// </summary>
        Memory,
        /// <summary>
        /// All data is written to a stream, this stream is managed by the framework, 
        /// but this is significantly less memory intensive than the Memory option.
        /// </summary>
        Stream
    }

    /// <summary>
    /// Represents the state of a network message.
    /// </summary>
    public class State : IDisposable
    {
        /// <summary>
        /// A <see cref="HttpWebRequest"/> for the message.
        /// </summary>
        public HttpWebRequest Request;
        /// <summary>
        /// A <see cref="HttpWebResponse"/> received from the host.
        /// </summary>
        public HttpWebResponse Response;
        /// <summary>
        /// A <see cref="DataStreamMethod"/> for this message.
        /// </summary>
        public DataStreamMethod DataStreamMethod;
        /// <summary>
        /// An <see cref="OperationType"/> for this message.
        /// </summary>
        public OperationType OperationType;
        /// <summary>
        /// A <see cref="Guid"/> that is a unique identifier for an asset.
        /// </summary>
        public Guid Guid;
        /// <summary>
        /// The size of the network buffer.
        /// </summary>
        public int BufferSize;
        /// <summary>
        /// The amount of time to elapse before a timeout happens.
        /// </summary>
        public int TimeoutDuration;
        /// <summary>
        /// The <see cref="Stream"/> for a <see cref="Message"/> to use.
        /// </summary>
        /// <remarks>
        /// If OperationType is GET/HEAD this is the Stream associated with the Response (OUT)
        /// If OperationType is PUT/POST this is the Stream associated with the Request (IN)
        /// </remarks>
        public Stream Stream;
        /// <summary>
        /// <c>True</c> if the stream is binary; otherwise, <c>false</c>.
        /// </summary>
        public bool StreamIsBinary;
        /// <summary>
        /// A accessor for the <see cref="WaitHandle"/>.
        /// </summary>
        public WaitHandle TimeoutWaitHandle;
        /// <summary>
        /// An accessor for the <see cref="RegisteredWaitHandle"/>.
        /// </summary>
        public RegisteredWaitHandle TimeoutRegisteredWaitHandle;
        /// <summary>
        /// A flag to indicate if a timeout has occurred.
        /// </summary>
        public bool IsTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        public State()
        {
            Request = null;
            Response = null;
            DataStreamMethod = DataStreamMethod.Stream;
            OperationType = OperationType.GET;
            Guid = Guid.Empty;
            BufferSize = 4096;
            TimeoutDuration = 1000;
            Stream = null;
            StreamIsBinary = false;
            TimeoutWaitHandle = null;
            TimeoutRegisteredWaitHandle = null;
            IsTimeout = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Request = null;
            if (Response != null) Response.Close();
            Response = null;
            if (Stream != null)
            {
                Stream.Close();
                Stream.Dispose();
            }
            if (TimeoutWaitHandle != null) TimeoutWaitHandle.Close();
            TimeoutRegisteredWaitHandle = null;
        }

        /// <summary>
        /// Gets a string for using by a <see cref="Logger"/> to document an event.
        /// </summary>
        /// <returns></returns>
        public string GetLogString()
        {
            string output = "";
            
            if(Request != null)
            {
                output += "Request\r\n" +
                    "\tAbsoluteUri=" + Request.RequestUri.AbsoluteUri + "\r\n" +
                    "\tAllowWriteStreamBuffering=" + Request.AllowWriteStreamBuffering.ToString() + "\r\n" +
                    "\tConnection=" + Request.Connection + "\r\n" +
                    "\tContentLength=" + Request.ContentLength.ToString() + "\r\n" +
                    "\tContentType=" + Request.ContentType + "\r\n" +
                    "\tKeepAlive=" + Request.KeepAlive.ToString() + "\r\n" +
                    "\tMethod=" + Request.Method + "\r\n" +
                    "\tProtocolVersion=" + Request.ProtocolVersion.ToString() + "\r\n" +
                    "\tSendChunked=" + Request.SendChunked.ToString() + "\r\n" +
                    "\tExpect100Continue=" + Request.ServicePoint.Expect100Continue.ToString() + "\r\n" +
                    "\tTimeout=" + Request.Timeout.ToString() + "\r\n" +
                    "\tTransferEncoding=" + Request.TransferEncoding + "\r\n" +
                    "\tUserAgent=" + Request.UserAgent + "\r\n" +
                    "\tHeaders=";
                
                if(Request.Headers.Count == 0)
                    output += "none\r\n";
                else
                {
                    for(int i=0; i<Request.Headers.Count; i++)
                        output += "\r\n\t\t" + Request.Headers[i] + "\r\n";
                }
            }

            if(Response != null)
            {
                output += "Response\r\n" +
                    "\tContentEncoding=" + Response.ContentEncoding + "\r\n" +
                    "\t=" + Response.ContentLength.ToString() + "\r\n" +
                    "\t=" + Response.ContentType + "\r\n" +
                    "\t=" + Response.IsFromCache.ToString() + "\r\n" +
                    "\t=" + Response.LastModified.ToString() + "\r\n" +
                    "\t=" + Response.Method + "\r\n" +
                    "\t=" + Response.ProtocolVersion.ToString() + "\r\n" +
                    "\t=" + Response.StatusCode.ToString() + "\r\n" +
                    "\t=" + Response.StatusDescription + "\r\n" +
                    "\tHeaders=";
                
                if(Request.Headers.Count == 0)
                    output += "none\r\n";
                else
                {
                    for(int i=0; i<Request.Headers.Count; i++)
                        output += "\r\n\t\t" + Request.Headers[i] + "\r\n";
                }
            }

            output += "DataStreamMethod=" + DataStreamMethod.ToString() + "\r\n" +
                "OperationType=" + OperationType.ToString() + "\r\n";

            if (Guid != Guid.Empty)
                output += "Guid=" + Guid.ToString("N") + "\r\n";

            output += "BufferSize=" + BufferSize.ToString() + "\r\n" +
                "TimeoutDuration=" + TimeoutDuration.ToString() + "\r\n" +
                "IsTimeout=" + IsTimeout.ToString();

            return output;
        }
    }
}
