using System;
using Netfox.Detective.ViewModelsDataEntity.BkTasks;

namespace Netfox.Detective.Interfaces
{
    public interface IBgTaskFactory
    {
        BgTaskVm Create(string title, string description, Action<BgTaskVm> taskFinishedCallBack);
    }
}