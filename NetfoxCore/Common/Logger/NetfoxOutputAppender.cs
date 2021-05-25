using System.Windows.Data;
using Castle.Core;
using log4net.Core;
using Netfox.Core.Collections;


namespace Netfox.Logger
{
    public class NetfoxOutputAppender : NetfoxAppenderBase
    {
        private readonly object _lock = new object();

        public NetfoxOutputAppender()
        {
            BindingOperations.EnableCollectionSynchronization(this.OutputMessages, this);
        }

        public ConcurrentObservableCollection<LoggingEvent> OutputMessages { get; } =
            new ConcurrentObservableCollection<LoggingEvent>();

        #region Overrides of NetfoxAppenderBase

        [DoNotWire] public override string Name { get; set; } = "Netfox Appender";

        public override void Close()
        {
            return;
        }

        protected override void DoAppendApproved(LoggingEvent loggingEvent)
        {
            lock (this._lock) this.OutputMessages.Add(loggingEvent);
        }

        #endregion
    }
}