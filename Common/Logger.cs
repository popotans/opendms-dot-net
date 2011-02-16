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
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Common
{
    /// <summary>
    /// Represents a facility for saving messages to a running list of messages.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Gets or sets the general logger.
        /// </summary>
        /// <value>
        /// The general logger.
        /// </value>
        public static ILog General { get; set; }
        /// <summary>
        /// Gets or sets the network logger.
        /// </summary>
        /// <value>
        /// The network logger.
        /// </value>
        public static ILog Network { get; set; }
        /// <summary>
        /// Gets or sets the security logger.
        /// </summary>
        /// <value>
        /// The security logger.
        /// </value>
        public static ILog Security { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger(string path)
        {
            log4net.Config.BasicConfigurator.Configure();

            General = LogManager.GetLogger("general.log");
            AddAppender(General, CreateRollingFileAppender(path, "general.log"));
            SetLevel(General, "ALL");

            Network = LogManager.GetLogger("network.log");
            AddAppender(Network, CreateRollingFileAppender(path, "network.log"));
            SetLevel(Network, "ALL");

            Security = LogManager.GetLogger("security.log");
            AddAppender(Security, CreateRollingFileAppender(path, "security.log"));
            SetLevel(Security, "ALL");
        }

        private void SetLevel(ILog log, string levelName)
        {
            log4net.Repository.Hierarchy.Logger logger = (log4net.Repository.Hierarchy.Logger)log.Logger;
            logger.Level = logger.Hierarchy.LevelMap[levelName];
        }

        private void AddAppender(ILog log, IAppender appender)
        {
            log4net.Repository.Hierarchy.Logger logger = (log4net.Repository.Hierarchy.Logger)log.Logger;
            logger.AddAppender(appender);
        }

        /// <summary>
        /// Creates a new Rolling File Appender at the specified path with the specified name.
        /// </summary>
        /// <param name="path">The directory path to the log file</param>
        /// <param name="logName">Name of the log.</param>
        private IAppender CreateRollingFileAppender(string path, string logName)
        {
            log4net.Appender.RollingFileAppender rfa = new log4net.Appender.RollingFileAppender();

            rfa.Name = logName;
            rfa.File = path + logName;
            rfa.AppendToFile = true;
            rfa.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
            rfa.MaxSizeRollBackups = 14;
            rfa.CountDirection = 1;
            rfa.MaximumFileSize = "15000KB";
            rfa.StaticLogFileName = true;
            rfa.Layout = new log4net.Layout.PatternLayout(@"%d [%t] %-5p %c %n%m%n------------------------------------------------------------------------------------------%n");
            rfa.ActivateOptions();
            rfa.Threshold = log4net.Core.Level.Debug;

            return rfa;
        }
    }
}
