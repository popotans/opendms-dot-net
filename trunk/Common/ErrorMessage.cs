using System;

namespace Common
{
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



        private int _id;
        private string _caption;
        private string _userMessage;
        private string _logMessage;
        private bool _saveToLog;
        private bool _displayToUser;
        private DateTime _timestamp;
        private Exception _exception;

        public int Id { get { return _id; } set { _id = value; } }
        public string Caption { get { return _caption; } set { _caption = value; } }
        public string UserMessage { get { return _userMessage; } set { _userMessage = value; } }
        public string LogMessage { get { return _logMessage; } set { _logMessage = value; } }
        public bool SaveToLog { get { return _saveToLog; } set { _saveToLog = value; } }
        public bool DisplayToUser { get { return _displayToUser; } set { _displayToUser = value; } }
        public DateTime Timestamp { get { return _timestamp; } }

        public ErrorMessage()
        {
            _timestamp = DateTime.Now;
        }

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

        public ErrorMessage(int id, string caption, string userMessage, string logMessage, bool saveToLog, bool displayToUser, Exception exception)
            : this(id, caption, userMessage, logMessage, saveToLog, displayToUser)
        {
            _exception = exception;
        }

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



        public static ErrorMessage TimeoutFailedToStart(Exception e, Work.JobBase job, string jobType)
        {
            return new ErrorMessage(TIMEOUT_FAILED_TO_START_CODE, TIMEOUT_FAILED_TO_START_CAPTION, TIMEOUT_FAILED_TO_START_USER,
                                    string.Format(TIMEOUT_FAILED_TO_START_LOG, job.Id.ToString(), jobType, ((Work.AssetJobBase)job).FullAsset.MetaAsset.GuidString), 
                                    true, true, e);
        }

        public static ErrorMessage GetETagFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_GETETAG_FAILED_CODE, JOB_GETETAG_FAILED_CAPTION, JOB_GETETAG_FAILED_USER,
                                    string.Format(JOB_GETETAG_FAILED_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true, e);
        }

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

        public static ErrorMessage LoadResourceFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_LOADRESOURCE_FAILED_CODE, JOB_LOADRESOURCE_FAILED_CAPTION, JOB_LOADRESOURCE_FAILED_USER,
                                    string.Format(JOB_LOADRESOURCE_FAILED_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true, e);
        }

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

        public static ErrorMessage JobCreateFailed(Exception e, Data.FullAsset fullAsset, ulong id)
        {
            return new ErrorMessage(JOB_CREATE_FAILED_CODE, JOB_CREATE_FAILED_CAPTION, JOB_CREATE_FAILED_USER,
                                    string.Format(JOB_CREATE_FAILED_LOG, id.ToString(), fullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        public static ErrorMessage JobStartFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_START_FAILED_CODE, JOB_START_FAILED_CAPTION, JOB_START_FAILED_USER,
                                    string.Format(JOB_START_FAILED_LOG, job.Id.ToString(), job.FullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        public static ErrorMessage JobRunFailed(Exception e, Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_RUN_FAILED_CODE, JOB_RUN_FAILED_CAPTION, JOB_RUN_FAILED_USER,
                                    string.Format(JOB_RUN_FAILED_LOG, job.Id.ToString(), job.FullAsset.MetaAsset.GuidString),
                                    true, true, e);
        }

        public static ErrorMessage JobInvalidResourceType(Work.AssetJobBase job)
        {
            return new ErrorMessage(JOB_INVALID_RESOURCE_TYPE_CODE, JOB_INVALID_RESOURCE_TYPE_CAPTION,
                                    JOB_INVALID_RESOURCE_TYPE_USER,
                                    string.Format(JOB_INVALID_RESOURCE_TYPE_LOG, job.FullAsset.MetaAsset.GuidString, job.Id.ToString()),
                                    true, true);
        }

        public static ErrorMessage WebNetFailed(Exception e, Network.State state)
        {
            return new ErrorMessage(WEB_NET_FAILED_CODE, WEB_NET_FAILED_CAPTION, WEB_NET_FAILED_USER,
                                    string.Format(WEB_NET_FAILED_LOG, state.Guid.ToString("N"), state.AssetType.ToString(), state.OperationType.ToString()),
                                    true, true, e);
        }

        public static ErrorMessage SearchOptionDownloadFailed(Exception e)
        {
            return new ErrorMessage(SEARCH_OPTIONSDOWNLOAD_FAILED_CODE, SEARCH_OPTIONSDOWNLOAD_FAILED_CAPTION, SEARCH_OPTIONSDOWNLOAD_FAILED_USER,
                                    SEARCH_OPTIONSDOWNLOAD_FAILED_LOG, true, true, e);
        }

        public static ErrorMessage SearchOptionDeserializationFailed(Exception e)
        {
            return new ErrorMessage(SEARCH_OPTIONSDESERIALIZATION_FAILED_CODE, SEARCH_OPTIONSDESERIALIZATION_FAILED_CAPTION, 
                                    SEARCH_OPTIONSDESERIALIZATION_FAILED_USER, SEARCH_OPTIONSDESERIALIZATION_FAILED_LOG, true, true, e);
        }
    }
}
