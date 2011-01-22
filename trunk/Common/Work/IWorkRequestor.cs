using System;

namespace Common.Work
{
    public interface IWorkRequestor
    {
        /// <summary>
        /// WorkReport accepts a UpdateUIDelegate and its associated arguments and should handle pumping this message to the UI
        /// </summary>
        /// <param name="actUpdateUI"></param>
        /// <param name="job"></param>
        /// <param name="fullAsset"></param>
        void WorkReport(JobBase.UpdateUIDelegate actUpdateUI, JobBase job, Data.FullAsset fullAsset);
    }
}
