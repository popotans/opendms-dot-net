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

namespace Common
{
    /// <summary>
    /// Represents an message explaining an error.
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// An enumeration of different types of errors that could occurr within the system.
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>
            /// Creation of the asset on the server failed.
            /// </summary>
            CreateResourceOnServerFailed,
            /// <summary>
            /// The timeout mechanism failed to start.
            /// </summary>
            TimeoutFailedToStart,
            /// <summary>
            /// Downloading of the meta asset from the server failed.
            /// </summary>
            DownloadMetaAssetFailed,
            /// <summary>
            /// Downloading of the data asset from the server failed.
            /// </summary>
            DownloadDataAssetFailed,
            /// <summary>
            /// An invalid state was encountered for the requested action.
            /// </summary>
            InvalidState,
            /// <summary>
            /// Failed to get the etag of the asset from the server.
            /// </summary>
            GetETagFailed,
            /// <summary>
            /// Failed to get the head of the asset from the server.
            /// </summary>
            GetHeadFailed,
            /// <summary>
            /// Failed to lock the asset on the server.
            /// </summary>
            LockAssetFailed,
            /// <summary>
            /// The job failed during instantiation.
            /// </summary>
            JobCreationFailed,
            /// <summary>
            /// The job failed to start.
            /// </summary>
            JobStartFailed,
            /// <summary>
            /// The job failed while processing work.
            /// </summary>
            JobWorkingFailed,
            /// <summary>
            /// Running of a job failed.
            /// </summary>
            JobRunFailed,
            /// <summary>
            /// Updating of the resource on the server failed.
            /// </summary>
            UpdateResourceFailed,
            /// <summary>
            /// Unlocking of the asset on the server failed.
            /// </summary>
            UnlockAssetFailed,
            /// <summary>
            /// Downloading of the available meta properties failed.
            /// </summary>
            MetaFormDownloadFailed,
            /// <summary>
            /// Deserialization of the available meta properties failed.
            /// </summary>
            MetaFormDeserializationFailed,
            /// <summary>
            /// Downloading of search options failed.
            /// </summary>
            SearchOptionDownloadFailed,
            /// <summary>
            /// Deserialization of search options failed.
            /// </summary>
            SearchOptionDeserializationFailed
        }
        
        /// <summary>
        /// The code of the error.
        /// </summary>
        private ErrorCode _code;
        /// <summary>
        /// The caption of the error message.
        /// </summary>
        private string _caption;
        /// <summary>
        /// The message displayed to the user.
        /// </summary>
        private string _userMessage;
        /// <summary>
        /// The message saved to the log.
        /// </summary>
        private string _logMessage;
        /// <summary>
        /// <c>True</c> to save the message to the log.
        /// </summary>
        private bool _saveToLog;
        /// <summary>
        /// <c>True</c> to display the message to the user.
        /// </summary>
        private bool _displayToUser;
        /// <summary>
        /// The timestamp of the error.
        /// </summary>
        private DateTime _timestamp;
        /// <summary>
        /// An exception that accompanied or caused the error.
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Gets or sets the code of the error.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public ErrorCode Code { get { return _code; } set { _code = value; } }
        /// <summary>
        /// Gets or sets the caption of the error message.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption { get { return _caption; } set { _caption = value; } }
        /// <summary>
        /// Gets or sets the message displayed to the user.
        /// </summary>
        /// <value>
        /// The user message.
        /// </value>
        public string UserMessage { get { return _userMessage; } set { _userMessage = value; } }
        /// <summary>
        /// Gets or sets the message saved to the log.
        /// </summary>
        /// <value>
        /// The log message.
        /// </value>
        public string LogMessage { get { return _logMessage; } set { _logMessage = value; } }
        /// <summary>
        /// Gets or sets a value indicating whether to save the message to the log.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to save the message to the log; otherwise, <c>false</c>.
        /// </value>
        public bool SaveToLog { get { return _saveToLog; } set { _saveToLog = value; } }
        /// <summary>
        /// Gets or sets a value indicating whether to display the message to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to display the message to the user; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayToUser { get { return _displayToUser; } set { _displayToUser = value; } }
        /// <summary>
        /// Gets the timestamp of the error.
        /// </summary>
        public DateTime Timestamp { get { return _timestamp; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        public ErrorMessage()
        {
            _timestamp = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="code">The code of the error.</param>
        /// <param name="caption">The caption of the error message.</param>
        /// <param name="userMessage">The message displayed to the user.</param>
        /// <param name="logMessage">The log message.</param>
        /// <param name="saveToLog">if set to <c>true</c> to save to log.</param>
        /// <param name="displayToUser">if set to <c>true</c> to display to user.</param>
        public ErrorMessage(ErrorCode code, string caption, string userMessage, string logMessage, bool saveToLog, bool displayToUser) 
            : this()
        {
            _code = code;
            _caption = caption;
            _userMessage = userMessage;
            _logMessage = logMessage;
            _saveToLog = saveToLog;
            _displayToUser = displayToUser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="code">The code of the error.</param>
        /// <param name="caption">The caption of the error message.</param>
        /// <param name="userMessage">The message displayed to the user.</param>
        /// <param name="logMessage">The log message.</param>
        /// <param name="saveToLog">if set to <c>true</c> to save to log.</param>
        /// <param name="displayToUser">if set to <c>true</c> to display to user.</param>
        /// <param name="exception">The exception that accompanied or caused the error.</param>
        public ErrorMessage(ErrorCode code, string caption, string userMessage, string logMessage, bool saveToLog, bool displayToUser, Exception exception)
            : this(code, caption, userMessage, logMessage, saveToLog, displayToUser)
        {
            _exception = exception;
        }

        /// <summary>
        /// Creates a string representation of this message to be saved to a log.
        /// </summary>
        /// <returns></returns>
        public string LogFormat()
        {
            string str =
                   "Id:           " + _code.ToString() + "\r\n" +
                   "Time:         " + _timestamp.ToString() + "\r\n" +
                   "Title:        " + _caption + "\r\n" +
                   "User Message: " + _userMessage + "\r\n" +
                   "Log Message:  " + _logMessage + "\r\n";

            if (_exception != null)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(_exception);
                str += "Stack Trace: " + st.ToString();
            }

            return str;
        }
    }
}
