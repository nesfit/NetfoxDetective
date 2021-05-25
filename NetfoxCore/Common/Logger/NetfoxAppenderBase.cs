using Castle.Core;
using Castle.Core.Logging;
using log4net.Appender;
using log4net.Core;

namespace Netfox.Logger
{
    public abstract class NetfoxAppenderBase : IAppender
    {
        [DoNotWire] public LoggerLevel LoggerLevel { get; set; } = LoggerLevel.Info;
        [DoNotWire] public abstract string Name { get; set; }
        public abstract void Close();
        protected abstract void DoAppendApproved(LoggingEvent loggingEvent);

        public void DoAppend(LoggingEvent loggingEvent)
        {
            if (LoggerLevel2Level(this.LoggerLevel) <= loggingEvent.Level)
                if (loggingEvent.RenderedMessage != null)
                    this.DoAppendApproved(loggingEvent);
        }

        public static Level LoggerLevel2Level(LoggerLevel loggerLevel)
        {
            return loggerLevel switch
            {
                LoggerLevel.Off => Level.Off,
                LoggerLevel.Fatal => Level.Fatal,
                LoggerLevel.Error => Level.Error,
                LoggerLevel.Warn => Level.Warn,
                LoggerLevel.Info => Level.Info,
                LoggerLevel.Debug => Level.Debug,
                _ => Level.All
            };
        }
    }
}