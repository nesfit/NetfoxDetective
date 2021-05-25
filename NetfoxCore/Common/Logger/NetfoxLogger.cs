using System;
using System.IO;
using Castle.Core.Logging;
using Castle.Services.Logging.Log4netIntegration;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace Netfox.Logger
{
    public class NetfoxLogger : ILogger, IDisposable
    {
        public NetfoxLogger(NetfoxFileAppender netfoxFileAppender, NetfoxOutputAppender netfoxOutputAppender)
        {
            this.NetfoxFileAppender = netfoxFileAppender;
            this.NetfoxOutputAppender = netfoxOutputAppender;
            this.CreateLogger("default");
        }

        public void CloseLoggingDirectory()
        {
            NetfoxFileAppender.Close();
        }

        public void ChangeLoggingDirectory(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                this.Logger?.Error($"ChangeLoggingDirectory failed - null {nameof(directoryInfo)}");
                return;
            }

            try
            {
                if (!directoryInfo.Exists) directoryInfo.Create();
            }
            catch (IOException e)
            {
                this.Logger?.Error("ChangeLoggingDirectory failed", e);
            }

            var oldLoggingDirectoryInfo = this.LoggingDirectory;
            try
            {
                this.LoggingDirectory = directoryInfo;
                this.NetfoxFileAppender.ChangeLoggingDirectory(directoryInfo);
            }
            catch (Exception e)
            {
                this.LoggingDirectory = oldLoggingDirectoryInfo;
                this.Logger?.Error("ChangeLoggingDirectory failed", e);
            }
        }

        public DirectoryInfo LoggingDirectory { get; private set; }

        public void CreateLogger(string loggerName)
        {
            //It will create a repository for each different arg it will receive
            var repositoryName = "Netfox";

            ILoggerRepository repository = null;

            var repositories = LogManager.GetAllRepositories();
            foreach (var loggerRepository in repositories)
            {
                if (loggerRepository.Name.Equals(repositoryName))
                {
                    repository = loggerRepository;
                    break;
                }
            }

            if (repository == null)
            {
                //Create a new repository
                repository = LogManager.CreateRepository(repositoryName);

                var hierarchy = (Hierarchy) repository;
                hierarchy.Root.Additivity = false;

                //Add appenders you need: here I need a rolling file and a memoryappender
                hierarchy.Root.AddAppender(this.NetfoxOutputAppender);
                hierarchy.Root.AddAppender(this.NetfoxFileAppender);


                BasicConfigurator.Configure(repository);
            }

            //Returns a logger from a particular repository;
            //Logger with same name but different repository will log using different appenders
            var logger = LogManager.GetLogger(repositoryName, loggerName);
            var logg = new ExtendedLog4netFactory();
            this.Logger = new ExtendedLog4netLogger(logger, logg);
        }

        public LoggerLevel BackgroundLoggerLevel
        {
            get => this.NetfoxFileAppender.LoggerLevel;
            set => this.NetfoxFileAppender.LoggerLevel = value;
        }

        public LoggerLevel ExplicitLoggerLevel
        {
            get => this.NetfoxOutputAppender.LoggerLevel;
            set => this.NetfoxOutputAppender.LoggerLevel = value;
        }

        private NetfoxFileAppender NetfoxFileAppender { get; set; }
        private NetfoxOutputAppender NetfoxOutputAppender { get; set; }

        public NetfoxLogger(string name) : this(name, LoggerLevel.Debug)
        {
        }

        public NetfoxLogger(string name, LoggerLevel level)
        {
            this.CreateLogger(name);
        }

        public ExtendedLog4netLogger Logger { get; set; }

        #region Implementation of ILogger

        public ILogger CreateChildLogger(string loggerName)
        {
            return this.Logger?.CreateChildLogger(loggerName);
        }

        public void Trace(string message)
        {
            this.Logger.Trace(message);
        }

        public void Trace(Func<string> messageFactory)
        {
            this.Logger.Trace(messageFactory);
        }

        public void Trace(string message, Exception exception)
        {
            this.Logger.Trace(message, exception);
        }

        public void TraceFormat(string format, params object[] args)
        {
            this.Logger.Trace(string.Format(format, args));
        }

        public void TraceFormat(Exception exception, string format, params object[] args)
        {
            this.Logger.Trace(string.Format(format, args), exception);
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger.Trace(string.Format(formatProvider, format, args));
        }

        public void TraceFormat(Exception exception, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            this.Logger.Trace(string.Format(formatProvider, format, args), exception);
        }

        public void Debug(string message)
        {
            this.Logger?.Debug(message);
        }

        public void Debug(Func<string> messageFactory)
        {
            this.Logger?.Debug(messageFactory);
        }

        public void Debug(string message, Exception exception)
        {
            this.Logger?.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this.Logger?.DebugFormat(format, args);
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            this.Logger?.DebugFormat(exception, format, args);
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.DebugFormat(formatProvider, format, args);
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            this.Logger?.DebugFormat(exception, formatProvider, format, args);
        }

        public void Error(string message)
        {
            this.Logger?.Error(message);
        }

        public void Error(Func<string> messageFactory)
        {
            this.Logger?.Error(messageFactory);
        }

        public void Error(string message, Exception exception)
        {
            this.Logger?.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this.Logger?.ErrorFormat(format, args);
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            this.Logger?.ErrorFormat(exception, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.ErrorFormat(formatProvider, format, args);
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            this.Logger?.ErrorFormat(exception, formatProvider, format, args);
        }

        public void Fatal(string message)
        {
            this.Logger?.Fatal(message);
        }

        public void Fatal(Func<string> messageFactory)
        {
            this.Logger?.Fatal(messageFactory);
        }

        public void Fatal(string message, Exception exception)
        {
            this.Logger?.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this.Logger?.FatalFormat(format, args);
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            this.Logger?.FatalFormat(exception, format, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.FatalFormat(formatProvider, format, args);
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            this.Logger?.FatalFormat(exception, formatProvider, format, args);
        }

        public void Info(string message)
        {
            this.Logger?.Info(message);
        }

        public void Info(Func<string> messageFactory)
        {
            this.Logger?.Info(messageFactory);
        }

        public void Info(string message, Exception exception)
        {
            this.Logger?.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this.Logger?.InfoFormat(format, args);
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            this.Logger?.InfoFormat(exception, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.InfoFormat(formatProvider, format, args);
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.InfoFormat(exception, formatProvider, format, args);
        }

        public void Warn(string message)
        {
            this.Logger?.Warn(message);
        }

        public void Warn(Func<string> messageFactory)
        {
            this.Logger?.Warn(messageFactory);
        }

        public void Warn(string message, Exception exception)
        {
            this.Logger?.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.Logger?.WarnFormat(format, args);
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            this.Logger?.WarnFormat(exception, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.WarnFormat(formatProvider, format, args);
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            this.Logger?.WarnFormat(exception, formatProvider, format, args);
        }

        public bool IsTraceEnabled { get; }
        public bool IsDebugEnabled => this.Logger.IsDebugEnabled;

        public bool IsErrorEnabled => this.Logger.IsErrorEnabled;

        public bool IsFatalEnabled => this.Logger.IsFatalEnabled;

        public bool IsInfoEnabled => this.Logger.IsInfoEnabled;

        public bool IsWarnEnabled => this.Logger.IsWarnEnabled;

        #region IDisposable

        public void Dispose()
        {
            this.NetfoxFileAppender.Close();
            this.NetfoxOutputAppender.Close();
        }

        #endregion

        #endregion
    }
}