using System;
using System.Windows.Controls;
using Netfox.Core.Collections;

namespace Netfox.Detective.Models
{
    /// <summary>
    ///     Log of operations anomaly from Netfox framework.
    /// </summary>
    public class OperationLog
    {
        public OperationLog()
        {
            this.LogItems = new ConcurrentObservableCollection<LogItem>();
        }

        public ConcurrentObservableCollection<LogItem> LogItems { get; set; }
        public string Name { get; set; }

        public void ToLog(LogItem item)
        {
            this.LogItems.AddBulk(item, 1000);
        }

        public void ToLog(LogItem.LogItemServility servility, string description, string detail = null)
        {
            this.LogItems.AddBulk(new LogItem(servility, description, detail), 1000);
        }


        public class LogItem
        {
            public enum LogItemServility
            {
                Info,
                Warning,
                Error,
                Critical
            }

            public LogItem()
            {
            }

            public LogItem(LogItemServility servility, string description, string detail = null)
            {
                this.TimeStamp = DateTime.Now;
                this.Servility = servility;
                this.Description = description;
                this.Detail = detail;
            }

            public DateTime TimeStamp { get; set; }
            public LogItemServility Servility { get; set; }
            public string Description { get; set; }

            public string Detail { get; set; }

            //public Conversation Conversation { get; set; }
            //public Capture Capture { get; set; }
            public Frame Frame { get; set; }
        }
    }
}