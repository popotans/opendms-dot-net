using System;

namespace Common.Work.Tasks
{
    public abstract class Base : ITask
    {
        protected int _percentComplete;

        public int PercentComplete
        {
            get { return _percentComplete; }
        }

        public delegate void TaskEventDelegate(Base sender);
        
        public event TaskEventDelegate OnProgress;
        public event TaskEventDelegate OnTimeout;

        public abstract bool Execute(Storage.Version resource, out string errorMessage);

        public void ReportProgress()
        {
            if (OnProgress != null) OnProgress(this);
        }
    }
}
