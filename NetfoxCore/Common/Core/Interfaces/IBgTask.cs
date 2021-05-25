using System;
using Netfox.Core.Enums;

namespace Netfox.Core.Interfaces
{
    public interface IBgTask
    {
        void Abort();
        int CompleteProgressValue { get; set; }
        int Progress { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        bool IsIndeterminate { get; set; }
        TaskState State { get; }
        DateTime StartTimeStamp { get; }
        TimeSpan Duration { get; }
        bool IsActive { get; }
        TaskResultState ResultState { get; }
        void Finish(TaskResultState resultState);
    }
}