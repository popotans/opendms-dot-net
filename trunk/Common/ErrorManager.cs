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
    /// <summary>
    /// Represents an object that manages errors.
    /// </summary>
    public class ErrorManager
    {
        /// <summary>
        /// Represents the method that handles an update UI event.
        /// </summary>
        /// <param name="errors">A collection of errors.</param>
        public delegate void UpdateUI(List<ErrorMessage> errors);

        /// <summary>
        /// A collection of errors.
        /// </summary>
        private List<ErrorMessage> _errors;
        /// <summary>
        /// A reference to the method handling update UI events.
        /// </summary>
        private UpdateUI _actUpdateUI;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorManager"/> class.
        /// </summary>
        /// <param name="actUpdateUI">A reference to the method handling update UI events.</param>
        public ErrorManager(UpdateUI actUpdateUI)
        {
            _errors = new List<ErrorMessage>();
            _actUpdateUI = actUpdateUI;
        }

        /// <summary>
        /// Adds the error.
        /// </summary>
        /// <param name="error">The error.</param>
        public void AddError(ErrorMessage error)
        {
            lock (_errors)
            {
                _errors.Add(error);
                Start();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(Run));
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
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

        /// <summary>
        /// Callback to end invocation.
        /// </summary>
        /// <param name="result">The result.</param>
        private void AsyncCallback(IAsyncResult result)
        {
            ((UpdateUI)result.AsyncState).EndInvoke(result);
        }

        /// <summary>
        /// Logs the errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        private void LogErrors(List<ErrorMessage> errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i].SaveToLog)
                {
                    Logger.General.Error(errors[i].LogFormat());
                }
            }
        }
    }
}
