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

namespace Common.CouchDB
{
    /// <summary>
    /// The Result class is used to represent the result of a method call
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates that the file exists
        /// </summary>
        public const int FILE_EXISTS        = 0x01;
        /// <summary>
        /// Indicates an invalid state
        /// </summary>
        public const int INVALID_STATE      = 0x02;

        /// <summary>
        /// True if the result is considered passing or successful, false otherwise
        /// </summary>
        public bool IsPass;

        /// <summary>
        /// A response message, if available or desired
        /// </summary>
        public string Message;

        /// <summary>
        /// An internal code that can be used to check for pre-determined types of problems (FILE_EXISTS, INVALID_STATE)
        /// </summary>
        public int Code;

        /// <summary>
        /// A reference to an exception if one occurred
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// A reference to a ServerResponse which indicates the result of a request against a CouchDB server
        /// </summary>
        public ServerResponse ServerResponse;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isPass">True if the result is considered passing or successful, false otherwise</param>
        public Result(bool isPass)
        {
            IsPass = isPass;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isPass">True if the result is considered passing or successful, false otherwise</param>
        /// <param name="message">A response message, if available or desired</param>
        public Result(bool isPass, string message)
        {
            IsPass = isPass;
            Message = message;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isPass">True if the result is considered passing or successful, false otherwise</param>
        /// <param name="message">A response message, if available or desired</param>
        /// <param name="code">An internal code that can be used to check for pre-determined types of problems</param>
        public Result(bool isPass, string message, int code)
        {
            IsPass = isPass;
            Message = message;
            Code = code;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isPass">True if the result is considered passing or successful, false otherwise</param>
        /// <param name="message">A response message, if available or desired</param>
        /// <param name="e">A reference to an exception if one occurred</param>
        public Result(bool isPass, string message, Exception e)
        {
            IsPass = isPass;
            Message = message;
            this.Exception = e;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sr">A reference to a ServerResponse which indicates the result of a request against a CouchDB server</param>
        public Result(ServerResponse sr)
        {
            if(sr.Ok == true)
                IsPass = true;
            else
                IsPass = false;
            Message = sr.Error + " - " + sr.Reason;
            ServerResponse = sr;
        }
    }
}
