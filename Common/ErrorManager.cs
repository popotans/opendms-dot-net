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
