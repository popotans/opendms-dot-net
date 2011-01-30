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
    public enum OperationType
    {
        NULL,
        GET,
        PUT,
        POST,
        DELETE,
        COPY,
        HEAD
    }

    public enum DataStreamMethod
    {
        Memory,
        Stream
    }

    public class State : IDisposable
    {
        public HttpWebRequest Request;
        public HttpWebResponse Response;
        public DataStreamMethod DataStreamMethod;
        public OperationType OperationType;
        public Guid Guid;
        public Common.Data.AssetType AssetType;
        public int BufferSize;
        public int TimeoutDuration;
        /// <summary>
        /// If OperationType is GET/HEAD this is the Stream associated with the Response (OUT)
        /// If OperationType is PUT/POST this is the Stream associated with the Request (IN)
        /// </summary>
        public Stream Stream;
        public bool StreamIsBinary;
        public WaitHandle TimeoutWaitHandle;
        public RegisteredWaitHandle TimeoutRegisteredWaitHandle;
        public bool IsTimeout;

        public State()
        {
            Request = null;
            Response = null;
            DataStreamMethod = DataStreamMethod.Stream;
            OperationType = OperationType.GET;
            Guid = Guid.Empty;
            AssetType = null;
            BufferSize = 4096;
            TimeoutDuration = 1000;
            Stream = null;
            StreamIsBinary = false;
            TimeoutWaitHandle = null;
            TimeoutRegisteredWaitHandle = null;
            IsTimeout = false;
        }

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

            if (!Data.AssetType.IsNullOrUnknown(AssetType))
                output += "AssetType=" + AssetType.VirtualPath.ToString() + "\r\n";

            output += "BufferSize=" + BufferSize.ToString() + "\r\n" +
                "TimeoutDuration=" + TimeoutDuration.ToString() + "\r\n" +
                "IsTimeout=" + IsTimeout.ToString();

            return output;
        }
    }
}
