// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System.Linq;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModelsDataEntity.BkTasks;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.BgTasks
{
    public class BgTasksManagerVm : DetectiveWindowViewModelBase
    {
        #region Constructors
        public BgTasksManagerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.ViewType = typeof(ITasksWindow);
            this.BgTasksManagerService.TaskUpdated += vm => this.TasksChanged();
            this.BgTasksManagerService.ActiveTasksUpdated += this.ActiveTaskManagerActiveTasksUpdated;
        }
        #endregion

        public override string HeaderText => "Task manager";

        #region Properties
        [IgnoreAutoChangeNotification]
        public ConcurrentObservableCollection<BgTaskVm> ActiveTasks { get; } = new ConcurrentObservableCollection<BgTaskVm>();

        [IgnoreAutoChangeNotification]
        public bool WholeIsIndeterminate => this.BgTasksManagerService.ActiveTasks.Where(t => t.IsActive).All(t => t.IsIndeterminate)
                                            && this.BgTasksManagerService.ActiveTasks.Any(t => t.IsIndeterminate);

        [IgnoreAutoChangeNotification]
        public int WholeProgress => this.BgTasksManagerService.ActiveTasks.Where(t => t.IsActive).Sum(t => (t.IsIndeterminate? 0 : t.Progress));

        [IgnoreAutoChangeNotification]
        public int WholeTarget => this.BgTasksManagerService.ActiveTasks.Where(t => t.IsActive).Sum(t => (t.IsIndeterminate? 0 : t.CompleteProgressValue));

        [IgnoreAutoChangeNotification]
        public int WholeRunning => this.BgTasksManagerService.ActiveTasks.Count(t => t.IsActive);

        [IgnoreAutoChangeNotification]
        public bool IsEnabled => this.BgTasksManagerService.ActiveTasks.Any(t => t.IsActive);
        #endregion

        #region Tasks creation handling
        private void ActiveTaskManagerActiveTasksUpdated(BgTasksManagerService.TaskUpdatedType type, BgTaskVm taskVm)
        {
            switch(type) {
                case BgTasksManagerService.TaskUpdatedType.Add:
                    this.ActiveTasks.Add(taskVm);
                    break;
                case BgTasksManagerService.TaskUpdatedType.Remove:
                    var tvm = this.ActiveTasks.FirstOrDefault(t => t.Equals(taskVm));
                    this.ActiveTasks.Remove(tvm);
                    break;
            }
            this.TasksChanged();
        }

        private void TasksChanged()
        {
            this.OnPropertyChanged(nameof(this.WholeIsIndeterminate));
            this.OnPropertyChanged(nameof(this.WholeProgress));
            this.OnPropertyChanged(nameof(this.WholeTarget));
            this.OnPropertyChanged(nameof(this.WholeRunning));
            this.OnPropertyChanged(nameof(this.IsEnabled));
        }
        
        #endregion
    }
}