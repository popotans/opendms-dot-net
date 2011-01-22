using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Common
{
    public class ErrorManager
    {
        public delegate void UpdateUI(List<ErrorMessage> errors);

        private List<ErrorMessage> _errors;
        private UpdateUI _actUpdateUI;
        private Logger _generalLogger;

        public ErrorManager(UpdateUI actUpdateUI, Logger generalLogger)
        {
            _errors = new List<ErrorMessage>();
            _actUpdateUI = actUpdateUI;
            _generalLogger = generalLogger;
        }

        public void AddError(ErrorMessage error)
        {
            lock (_errors)
            {
                _errors.Add(error);
                Start();
            }
        }

        public void Start()
        {
            Thread t = new Thread(new ThreadStart(Run));
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        private void Run()
        {
            if (_errors == null || _errors.Count <= 0)
                return;

            lock (_errors)
            {
                LogErrors(_errors);
                _actUpdateUI.BeginInvoke(_errors, AsyncCallback, _actUpdateUI);
            }
        }

        private void AsyncCallback(IAsyncResult result)
        {
            ((UpdateUI)result.AsyncState).EndInvoke(result);
        }

        private void LogErrors(List<ErrorMessage> errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i].SaveToLog)
                {
                    _generalLogger.Write(Logger.LevelEnum.Normal, errors[i].LogFormat());
                }
            }
        }
    }
}
