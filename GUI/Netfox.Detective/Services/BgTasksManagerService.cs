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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModelsDataEntity.BkTasks;

namespace Netfox.Detective.Services
{
    /// <summary>
    ///     Managed tasks administration.
    /// </summary>
    public class BgTasksManagerService : DetectiveApplicationServiceBase, IDisposable, IBgTasksManagerService
    {
        public IBgTaskFactory BgTaskFactory { get; }

        public BgTasksManagerService(IBgTaskFactory bgTaskFactory)
        {
            this.BgTaskFactory = bgTaskFactory;
            this.AdminstrateTasksCancellationToken = new CancellationTokenSource();
            this.AdminstrateTasksTask = PeriodicalRepetitiveTask.Create((now, ct) => this.AdminstrateTasks(ct), this.AdminstrateTasksCancellationToken.Token, TimeSpan.FromSeconds(1));
            this.AdminstrateTasksTask.Post(DateTimeOffset.Now);
        }

        private ActionBlock<DateTimeOffset> AdminstrateTasksTask { get; set; }

        private CancellationTokenSource AdminstrateTasksCancellationToken { get; set; }

        public IBgTask CreateTask(string title, string description, CancellationToken cancellationToken)
        {
            lock (this.ActiveTasks)
            {
                var newTaskVm = this.BgTaskFactory.Create(title, description, this.TaskFinishedCallBack);
                this.AddTask(newTaskVm);
                return newTaskVm;
            }
        }

        public void AddTask(BgTaskVm newTaskVm)
        {
            //newTaskVm.PropertyChanged += this.Task_PropertyChanged;
            this.ActiveTasks.Add(newTaskVm);
            this.ActiveTasksUpdated?.Invoke(TaskUpdatedType.Add, newTaskVm);
        }

        #region Events and handlers
        public delegate void TaskUpdatedHandler(BgTaskVm updatedTaskVm);
#pragma warning disable 0067
        public event TaskUpdatedHandler TaskUpdated;
#pragma warning restore 0067
        public enum TaskUpdatedType
        {
            Add,
            Remove,
            Changed
        }

        private void TaskFinishedCallBack(BgTaskVm task)
        {
            this.ActiveTasksUpdated?.Invoke(TaskUpdatedType.Changed, task);
        }

        public delegate void TasksUpdatedHandler(TaskUpdatedType type, BgTaskVm taskVm);

        public event TasksUpdatedHandler ActiveTasksUpdated;
        #endregion

        public void Stop()
        {
            this.AdminstrateTasksCancellationToken.Cancel();
        }

        #region Properties
   
        public ConcurrentObservableCollection<BgTaskVm> ActiveTasks { get; } = new ConcurrentObservableCollection<BgTaskVm>();
        #endregion

        #region Active task administration

        // Periodic check of tasks.
        private async Task AdminstrateTasks(CancellationToken ct)
        {
            await Task.Run(() => {
                foreach (var task in this.ActiveTasks.Where(t => t.IsActive))
                {
                    if (ct.IsCancellationRequested) return;
                    task.UpdateDuration();
                }
            }, ct);
            
        }
        #endregion

        #region Implementation of IDisposable
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            this.AdminstrateTasksCancellationToken.Cancel();
        }

        ~BgTasksManagerService()
        {
            this.Dispose(false);
        }

        
        #endregion
    }
}