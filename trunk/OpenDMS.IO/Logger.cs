using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace OpenDMS.IO
{
    public class Logger
    {
		#region Constructors (1) 

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger(string path)
        {
            log4net.Config.BasicConfigurator.Configure();

            FileSystem = LogManager.GetLogger("filesystem.log");
            AddAppender(FileSystem, CreateRollingFileAppender(path, "filesystem.log"));
            SetLevel(FileSystem, "ALL");
        }

		#endregion Constructors 

		#region Properties (1) 

        /// <summary>
        /// Gets or sets the file system logger.
        /// </summary>
        /// <value>
        /// The file system logger.
        /// </value>
        public static ILog FileSystem { get; set; }

		#endregion Properties 

		#region Methods (3) 

		// Private Methods (3) 

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

        private void SetLevel(ILog log, string levelName)
        {
            log4net.Repository.Hierarchy.Logger logger = (log4net.Repository.Hierarchy.Logger)log.Logger;
            logger.Level = logger.Hierarchy.LevelMap[levelName];
        }

		#endregion Methods 
    }
}
