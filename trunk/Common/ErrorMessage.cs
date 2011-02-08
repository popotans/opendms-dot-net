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
        // Timeout Failed 1
        public const int TIMEOUT_FAILED_TO_START_CODE = 0x00001;        
        public const string TIMEOUT_FAILED_TO_START_CAPTION = "Failed to Start Timeout";
        public const string TIMEOUT_FAILED_TO_START_USER = "The system failed to start an operation to prevent the system from locking up when another operation takes to long.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string TIMEOUT_FAILED_TO_START_LOG = "Timeout failed to start on job id {0} which was processing a job type of {1} on the resource id {2}";

        // Web Network (WebException) Failed 2
        public const int WEB_NET_FAILED_CODE = 0x00010;
        public const string WEB_NET_FAILED_CAPTION = "Network Communications Failed";
        public const string WEB_NET_FAILED_USER = "The system failed to transfer the required data.  The system will stop attempting this transfer, you might need to retry.";
        public const string WEB_NET_FAILED_LOG = "Web failed to transfer the required data, on resource id {0} with asset type {1} performing operation type {2}";

        // Search Options Download Failed 3
        public const int SEARCH_OPTIONSDOWNLOAD_FAILED_CODE = 0x00011;
        public const string SEARCH_OPTIONSDOWNLOAD_FAILED_CAPTION = "Search Options Download Failed";
        public const string SEARCH_OPTIONSDOWNLOAD_FAILED_USER = "The system failed to download search options.  The system will stop attempting this transfer, you might need to close the search window and retry.";
        public const string SEARCH_OPTIONSDOWNLOAD_FAILED_LOG = "Search options failed to download.";

        // Search Options Deserialize Failed 4
        public const int SEARCH_OPTIONSDESERIALIZATION_FAILED_CODE = 0x00100;
        public const string SEARCH_OPTIONSDESERIALIZATION_FAILED_CAPTION = "Reading Search Options Failed";
        public const string SEARCH_OPTIONSDESERIALIZATION_FAILED_USER = "The system cannot understand the search options provided by the server.  The system will stop attempting this transfer, you might need to close the search window and retry.";
        public const string SEARCH_OPTIONSDESERIALIZATION_FAILED_LOG = "Search options failed deserialization.";

        // Meta Form Download Failed 5
        public const int METAFORM_DOWNLOAD_FAILED_CODE = 0x00101;
        public const string METAFORM_DOWNLOAD_FAILED_CAPTION = "Meta Properties Form Download Failed";
        public const string METAFORM_DOWNLOAD_FAILED_USER = "The system failed to download the meta properties form.  The system will stop attempting this transfer, you might need to cancel and retry.";
        public const string METAFORM_DOWNLOAD_FAILED_LOG = "Meta properties form failed to download.";

        // Meta Form Deserialize Failed 6
        public const int METAFORM_DESERIALIZATION_FAILED_CODE = 0x00110;
        public const string METAFORM_DESERIALIZATION_FAILED_CAPTION = "Reading Meta Form Properties Failed";
        public const string METAFORM_DESERIALIZATION_FAILED_USER = "The system cannot understand the meta form properties provided by the server.  The system will stop attempting this transfer, you might need to cancel and retry.";
        public const string METAFORM_DESERIALIZATION_FAILED_LOG = "Meta form failed deserialization.";

        // JOBS

        // Job Creation 1
        public const int JOB_CREATE_FAILED_CODE = 0x10001;
        public const string JOB_CREATE_FAILED_CAPTION = "Failed to Create a Job";
        public const string JOB_CREATE_FAILED_USER = "The system failed to perform an operation.  The system will stop attempting to perform this operation.  You might need to retry.";
        public const string JOB_CREATE_FAILED_LOG = "Failed while instantiating a job, job id {0} operating on resource id {1}";

        // Job Start 2
        public const int JOB_START_FAILED_CODE = 0x10010;
        public const string JOB_START_FAILED_CAPTION = "Failed to Start a Job";
        public const string JOB_START_FAILED_USER = "The system failed to properly start an operation.  The system will stop attempting to perform this operation.  You might need to retry.";
        public const string JOB_START_FAILED_LOG = "Failed while starting a job, job id {0} operating on resource id {1}";

        // Job Run 3
        public const int JOB_RUN_FAILED_CODE = 0x10011;
        public const string JOB_RUN_FAILED_CAPTION = "Failed to Run a Job";
        public const string JOB_RUN_FAILED_USER = "The system failed to properly run an operation.  The system will stop attempting to perform this operation.  You might need to retry.";
        public const string JOB_RUN_FAILED_LOG = "Failed while running a job, job id {0} operating on resource id {1}";

        // Job GetETag Failed 4
        public const int JOB_GETETAG_FAILED_CODE = 0x10100;
        public const string JOB_GETETAG_FAILED_CAPTION = "Failed to Retrieve the ETag";
        public const string JOB_GETETAG_FAILED_USER = "The system failed to retrieve an identifier indicating if the resource is outdated.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_GETETAG_FAILED_LOG = "GetETagJob failed to retrieve the ETag for the remote resource with id {0} on using job id {1}";

        // Job LoadResource Failed 5
        public const int JOB_LOADRESOURCE_FAILED_CODE = 0x10101;
        public const string JOB_LOADRESOURCE_FAILED_CAPTION = "Failed to Retrieve the Resource";
        public const string JOB_LOADRESOURCE_FAILED_USER = "The system failed to retrieve a resource.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_LOADRESOURCE_FAILED_LOG = "LoadResourceJob failed to retrieve the remote resource with id {0} on using job id {1}";

        // Job InvalidResourceType 6
        public const int JOB_INVALID_RESOURCE_TYPE_CODE = 0x10110;
        public const string JOB_INVALID_RESOURCE_TYPE_CAPTION = "An invalid resource type was received.";
        public const string JOB_INVALID_RESOURCE_TYPE_USER = "The system received invalid input as part of its programming, please report this to the programmer.";
        public const string JOB_INVALID_RESOURCE_TYPE_LOG = "SaveResourceJob was given a resource type that was not of type Data.LocalResource, this was on resource id {0] running with a job id {0}";

        // Job SaveResource Failed 7
        public const int JOB_SAVERESOURCE_FAILED_CODE = 0x10111;
        public const string JOB_SAVERESOURCE_FAILED_CAPTION = "Failed to Save the Resource";
        public const string JOB_SAVERESOURCE_FAILED_USER = "The system failed to save a resource.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_SAVERESOURCE_FAILED_LOG = "SaveResourceJob.Run failed to save the remote resource with id {0} on using job id {1}";
        
        // Job GetETag Failed due to Invalid State 8
        public const int JOB_GETETAG_FAILED_STATE_CODE = 0x11000;
        public const string JOB_GETETAG_FAILED_STATE_CAPTION = "Failed to Retrieve the ETag";
        public const string JOB_GETETAG_FAILED_STATE_USER = "The system failed to retrieve an identifier indicating if the resource is outdated.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_GETETAG_FAILED_STATE_LOG = "GetETagJob failed because the MetaAsset was not properly set for the remote resource with id {0} on using job id {1}";

        // Job SaveResource Failed 9
        public const int JOB_CREATERESOURCE_FAILED_CODE = 0x11001;
        public const string JOB_CREATERESOURCE_FAILED_CAPTION = "Failed to Create the Resource";
        public const string JOB_CREATERESOURCE_FAILED_USER = "The system failed to create a resource.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_CREATERESOURCE_FAILED_LOG = "CreateResourceJob.Run failed to save the remote resource with id {0} on using job id {1}";

        // Job GetHead Failed 10
        public const int JOB_GETHEAD_FAILED_CODE = 0x11010;
        public const string JOB_GETHEAD_FAILED_CAPTION = "Failed to Retrieve the Header information";
        public const string JOB_GETHEAD_FAILED_USER = "The system failed to retrieve an identifier indicating if the resource is outdated.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_GETHEAD_FAILED_LOG = "GetHeadJob failed to retrieve the Header information for the remote resource with id {0} on using job id {1}";

        // Job GetHead Failed due to Invalid State 11
        public const int JOB_GETHEAD_FAILED_STATE_CODE = 0x11000;
        public const string JOB_GETHEAD_FAILED_STATE_CAPTION = "Failed to Retrieve the Header information";
        public const string JOB_GETHEAD_FAILED_STATE_USER = "The system failed to retrieve an identifier indicating if the resource is outdated.  The system will stop attempting to perform the current action.  You might need to retry your action.";
        public const string JOB_GETHEAD_FAILED_STATE_LOG = "GetHeadJob failed because the MetaAsset was not properly set for the remote resource with id {0} on using job id {1}";



        /// <summary>
        /// The code of the error.
        /// </summary>
        private int _id;
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
        public int Id { get { return _id; } set { _id = value; } }
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
        /// <param name="id">The code of the error.</param>
        /// <param name="caption">The caption of the error message.</param>
        /// <param name="userMessage">The message displayed to the user.</param>
        /// <param name="logMessage">The log message.</param>
        /// <param name="saveToLog">if set to <c>true</c> to save to log.</param>
        /// <param name="displayToUser">if set to <c>true</c> to display to user.</param>
        public ErrorMessage(int id, string caption, string userMessage, string logMessage, bool saveToLog, bool displayToUser) 
            : this()
        {
            _id = id;
            _caption = caption;
            _userMessage = userMessage;
            _logMessage = logMessage;
            _saveToLog = saveToLog;
            _displayToUser = displayToUser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="id">The code of the error.</param>
        /// <param name="caption">The caption of the error message.</param>
        /// <param name="userMessage">The message displayed to the user.</param>
        /// <param name="logMessage">The log message.</param>
        /// <param name="saveToLog">if set to <c>true</c> to save to log.</param>
        /// <param name="displayToUser">if set to <c>true</c> to display to user.</param>
        /// <param name="exception">The exception that accompanied or caused the error.</param>
        public ErrorMessage(int id, string caption, string userMessage, string logMessage, bool saveToLog, bool displayToUser, Exception exception)
            : this(id, caption, userMessage, logMessage, saveToLog, displayToUser)
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
                   "Id:           " + _id.ToString() + "\r\n" +
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



        /// <summary>
        /// Creates a timeout failed to start error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage TimeoutFailedToStart(Exception e, Work.JobBase job, string jobType)
        {
            return new ErrorMessage(TIMEOUT_FAILED_TO_START_CODE, TIMEOUT_FAILED_TO_START_CAPTION, TIMEOUT_FAILED_TO_START_USER,
                                    string.Format(TIMEOUT_FAILED_TO_START_LOG, job.Id.ToString(), jobType, ((Work.AssetJobBase)job).FullAsset.MetaAsset.GuidString), 
                                    true, true, e);
        }

        /// <summary>
        /// Creates a get head failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage GetHeadFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_GETHEAD_FAILED_CODE, JOB_GETHEAD_FAILED_CAPTION, JOB_GETHEAD_FAILED_USER,
                                    string.Format(JOB_GETHEAD_FAILED_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a get etag failed due to invalid state error message.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage GetHeadFailedDueToInvalidState(Work.AssetJobBase job)
        {
            if (job.FullAsset != null && job.FullAsset.MetaAsset != null)
                return new ErrorMessage(JOB_GETHEAD_FAILED_STATE_CODE, JOB_GETHEAD_FAILED_STATE_CAPTION,
                                    JOB_GETHEAD_FAILED_STATE_USER,
                                    string.Format(JOB_GETHEAD_FAILED_STATE_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);

            return new ErrorMessage(JOB_GETHEAD_FAILED_STATE_CODE, JOB_GETHEAD_FAILED_STATE_CAPTION,
                                    JOB_GETHEAD_FAILED_STATE_USER,
                                    string.Format(JOB_GETHEAD_FAILED_STATE_LOG, "Unknown", job.Id.ToString()),
                                    true, true);
        }

        /// <summary>
        /// Creates a get etag failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage GetETagFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_GETETAG_FAILED_CODE, JOB_GETETAG_FAILED_CAPTION, JOB_GETETAG_FAILED_USER,
                                    string.Format(JOB_GETETAG_FAILED_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a get etag failed due to invalid state error message.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage GetETagFailedDueToInvalidState(Work.AssetJobBase job)
        {
            if(job.FullAsset != null && job.FullAsset.MetaAsset != null)
                return new ErrorMessage(JOB_GETETAG_FAILED_STATE_CODE, JOB_GETETAG_FAILED_STATE_CAPTION, 
                                    JOB_GETETAG_FAILED_STATE_USER,
                                    string.Format(JOB_GETETAG_FAILED_STATE_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);

            return new ErrorMessage(JOB_GETETAG_FAILED_STATE_CODE, JOB_GETETAG_FAILED_STATE_CAPTION,
                                    JOB_GETETAG_FAILED_STATE_USER,
                                    string.Format(JOB_GETETAG_FAILED_STATE_LOG, "Unknown", job.Id.ToString()),
                                    true, true);
        }

        /// <summary>
        /// Creates a load resource failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage LoadResourceFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_LOADRESOURCE_FAILED_CODE, JOB_LOADRESOURCE_FAILED_CAPTION, JOB_LOADRESOURCE_FAILED_USER,
                                    string.Format(JOB_LOADRESOURCE_FAILED_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a save resource failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <param name="additionalMessageUser">The additional message to display to user.</param>
        /// <param name="additionalMessageLog">The additional message to save to the log.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage SaveResourceFailed(Exception e, Work.AssetJobBase job, string additionalMessageUser,
            string additionalMessageLog)
        {
            if (e != null)
                return new ErrorMessage(JOB_SAVERESOURCE_FAILED_CODE, JOB_SAVERESOURCE_FAILED_CAPTION,
                                        JOB_SAVERESOURCE_FAILED_USER + "  " + additionalMessageUser,
                                        string.Format(JOB_SAVERESOURCE_FAILED_LOG + "  " + additionalMessageLog,
                                            job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                        true, true, e);

            return new ErrorMessage(JOB_SAVERESOURCE_FAILED_CODE, JOB_SAVERESOURCE_FAILED_CAPTION,
                                    JOB_SAVERESOURCE_FAILED_USER + "  " + additionalMessageUser,
                                    string.Format(JOB_SAVERESOURCE_FAILED_LOG + "  " + additionalMessageLog,
                                        job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);
        }

        /// <summary>
        /// Creates a create resource failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <param name="additionalMessageUser">The additional message to display to user.</param>
        /// <param name="additionalMessageLog">The additional message to save to the log.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage CreateResourceFailed(Exception e, Work.AssetJobBase job, string additionalMessageUser,
            string additionalMessageLog)
        {
            if (e != null)
                return new ErrorMessage(JOB_CREATERESOURCE_FAILED_CODE, JOB_CREATERESOURCE_FAILED_CAPTION,
                                        JOB_CREATERESOURCE_FAILED_USER + "  " + additionalMessageUser,
                                        string.Format(JOB_CREATERESOURCE_FAILED_LOG + "  " + additionalMessageLog,
                                            job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                        true, true, e);

            return new ErrorMessage(JOB_CREATERESOURCE_FAILED_CODE, JOB_CREATERESOURCE_FAILED_CAPTION,
                                    JOB_CREATERESOURCE_FAILED_USER + "  " + additionalMessageUser,
                                    string.Format(JOB_CREATERESOURCE_FAILED_LOG + "  " + additionalMessageLog,
                                        job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);
        }

        /// <summary>
        /// Creates a job create failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="fullAsset">The full asset.</param>
        /// <param name="id">The code id.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage JobCreateFailed(Exception e, Data.FullAsset fullAsset, ulong id)
        {
            return new ErrorMessage(JOB_CREATE_FAILED_CODE, JOB_CREATE_FAILED_CAPTION, JOB_CREATE_FAILED_USER,
                                    string.Format(JOB_CREATE_FAILED_LOG, id.ToString(), fullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a job start failed error message
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage JobStartFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_START_FAILED_CODE, JOB_START_FAILED_CAPTION, JOB_START_FAILED_USER,
                                    string.Format(JOB_START_FAILED_LOG, job.Id.ToString(), job.FullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a job run failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage JobRunFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_RUN_FAILED_CODE, JOB_RUN_FAILED_CAPTION, JOB_RUN_FAILED_USER,
                                    string.Format(JOB_RUN_FAILED_LOG, job.Id.ToString(), job.FullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a job invalid resource type error message.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage JobInvalidResourceType(Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_INVALID_RESOURCE_TYPE_CODE, JOB_INVALID_RESOURCE_TYPE_CAPTION,
                                    JOB_INVALID_RESOURCE_TYPE_USER,
                                    string.Format(JOB_INVALID_RESOURCE_TYPE_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);
        }

        /// <summary>
        /// Creates a web net failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="state">The <see cref="Network.State"/>.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage WebNetFailed(Exception e, Network.State state)
        {
            return new ErrorMessage(WEB_NET_FAILED_CODE, WEB_NET_FAILED_CAPTION, WEB_NET_FAILED_USER,
                                    string.Format(WEB_NET_FAILED_LOG, state.Guid.ToString("N"), state.AssetType.ToString(), state.OperationType.ToString()),
                                    true, true, e);
        }

        /// <summary>
        /// Creates a search option download failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage SearchOptionDownloadFailed(Exception e)
        {
            return new ErrorMessage(SEARCH_OPTIONSDOWNLOAD_FAILED_CODE, SEARCH_OPTIONSDOWNLOAD_FAILED_CAPTION, SEARCH_OPTIONSDOWNLOAD_FAILED_USER,
                                    SEARCH_OPTIONSDOWNLOAD_FAILED_LOG, true, true, e);
        }

        /// <summary>
        /// Creates a search option deserialization failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage SearchOptionDeserializationFailed(Exception e)
        {
            return new ErrorMessage(SEARCH_OPTIONSDESERIALIZATION_FAILED_CODE, SEARCH_OPTIONSDESERIALIZATION_FAILED_CAPTION, 
                                    SEARCH_OPTIONSDESERIALIZATION_FAILED_USER, SEARCH_OPTIONSDESERIALIZATION_FAILED_LOG, true, true, e);
        }

        /// <summary>
        /// Creates a meta form download failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage MetaFormDownloadFailed(Exception e)
        {
            return new ErrorMessage(METAFORM_DOWNLOAD_FAILED_CODE, METAFORM_DOWNLOAD_FAILED_CAPTION, METAFORM_DOWNLOAD_FAILED_USER,
                                    METAFORM_DOWNLOAD_FAILED_LOG, true, true, e);
        }

        /// <summary>
        /// Creates a meta form deserialization failed error message.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>An <see cref="ErrorMessage"/>.</returns>
        public static ErrorMessage MetaFormDeserializationFailed(Exception e)
        {
            return new ErrorMessage(METAFORM_DESERIALIZATION_FAILED_CODE, METAFORM_DESERIALIZATION_FAILED_CAPTION,
                                    METAFORM_DESERIALIZATION_FAILED_USER, METAFORM_DESERIALIZATION_FAILED_LOG, true, true, e);
        }
    }
}
