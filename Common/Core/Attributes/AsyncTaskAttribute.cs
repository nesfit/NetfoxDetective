// Copyright (c) 2017 Jan Pluskal
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Castle.Windsor;
using Netfox.Core.Interfaces;
using Netfox.Core.Messages;
using PostSharp.Aspects;

namespace Netfox.Core.Attributes
{
  
    [Serializable]
    public class AsyncTaskAttribute : OnMethodBoundaryAspect, ILoggable
    {
        public AsyncTaskAttribute() { this.ApplyToStateMachine = false; }
        public WindsorContainer WindsorContainer { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public override void OnEntry(MethodExecutionArgs args)
        {
            var windsorPropertyInfo = args.Instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).FirstOrDefault(p => p.PropertyType == typeof(WindsorContainer));
            if (windsorPropertyInfo != null) { this.WindsorContainer = windsorPropertyInfo.GetValue(args.Instance) as WindsorContainer; }
            this.ResolveLogger();

            this.Logger?.Info($"{this.Title} - Started, {this.Description}");

            this.RegisterWithTaskManager(args);
        }

        private void ResolveLogger()
        {
            if (!this.WindsorContainer?.Kernel.HasComponent(typeof(ILogger)) ?? false) return;
            this.Logger =  this.WindsorContainer?.Resolve<ILogger>();
        }

        public ILogger Logger { get; set; }

        private void RegisterWithTaskManager(MethodExecutionArgs args)
        {
            if(!this.WindsorContainer?.Kernel.HasComponent(typeof(IBgTasksManagerService)) ?? false) return;

            try
            {
                var bgTasksManagerService = this.WindsorContainer?.Resolve<IBgTasksManagerService>();
                var cancelationToken = args.Arguments.FirstOrDefault(a => a is CancellationToken);
                if(cancelationToken != null) { this._cancelationToken = (CancellationToken) cancelationToken; }
                this.BgTaskVm = bgTasksManagerService?.CreateTask(this.Title, this.Description, this._cancelationToken);
            }
            catch(Exception ex) {
                this.Logger?.Error($"{this.Title} - Error in task registration",ex);
            }
        }

        public IBgTask BgTaskVm { get; private set; }
        [NonSerialized] private CancellationToken _cancelationToken;
        public override void OnSuccess(MethodExecutionArgs args)
        {
            try
            {
                var task = (Task)args.ReturnValue;
                task.ContinueWith(t =>
                {
                    this.Logger?.Info($"{this.Title} - Finished, {this.Description}");
                    this.BgTaskVm?.Finish(TaskResultState.Ok);
                }, this._cancelationToken);
            }
            catch (Exception ex)
            {
                this.Logger?.Info($"{this.Title} - Exception, {ex.Message}");
                this.BgTaskVm?.Finish(TaskResultState.Error);
            }
        }
    }
}
