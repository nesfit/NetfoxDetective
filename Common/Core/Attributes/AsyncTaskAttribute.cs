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
using Castle.Core.Logging;
using Castle.Windsor;
using Netfox.Core.Interfaces;
using PostSharp.Aspects;

namespace Netfox.Core.Attributes
{
    [Serializable]
    public class AsyncTaskAttribute : OnMethodBoundaryAspect, ILoggable
    {
        [NonSerialized] private CancellationToken _cancellationToken;
        public AsyncTaskAttribute() { this.SemanticallyAdvisedMethodKinds = SemanticallyAdvisedMethodKinds.Async; }
        public IWindsorContainer WindsorContainer { get; set; }
        public String Title { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;

        public IBgTask BgTaskVm { get; private set; }

        public ILogger Logger { get; set; }

        public override void OnEntry(MethodExecutionArgs args)
        {
            var windsorPropertyInfo = args.Instance.GetType().GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.GetProperty)
                .FirstOrDefault(p => p.PropertyType == typeof(WindsorContainer) || p.PropertyType == typeof(IWindsorContainer));
            if(windsorPropertyInfo != null) this.WindsorContainer = windsorPropertyInfo.GetValue(args.Instance) as WindsorContainer;
            this.ResolveLogger();

            this.LogMethodStart();

            this.RegisterWithTaskManager(args);
        }

        private void LogMethodStart() { this.Logger?.Info($"{this?.Title} - Started, {this?.Description}"); }

        #region Overrides of OnMethodBoundaryAspect
        public override void OnException(MethodExecutionArgs args)
        {
            this.LogMethodException(args);
            this.ReportErrorToTaskManager();
        }

        private void ReportErrorToTaskManager() { this.BgTaskVm?.Finish(TaskResultState.Error); }

        private void LogMethodException(MethodExecutionArgs args) { this.Logger?.Info($"{this?.Title} - Exception, {args.Exception.Message}"); }
        #endregion

        public override void OnSuccess(MethodExecutionArgs args)
        {
            this.LogMethodSuccessfulFinish();
            this.FinishTaskInTaskManager();
        }

        private void FinishTaskInTaskManager() { this.BgTaskVm?.Finish(TaskResultState.Ok); }

        private void LogMethodSuccessfulFinish() { this.Logger?.Info($"{this?.Title} - Finished, {this?.Description}"); }

        private void RegisterWithTaskManager(MethodExecutionArgs args)
        {
            if(!this.WindsorContainer?.Kernel.HasComponent(typeof(IBgTasksManagerService)) ?? false) return;

            try
            {
                var bgTasksManagerService = this.WindsorContainer?.Resolve<IBgTasksManagerService>();
                var cancelationToken = args.Arguments.FirstOrDefault(a => a is CancellationToken);
                if(cancelationToken != null) this._cancellationToken = (CancellationToken) cancelationToken;
                this.BgTaskVm = bgTasksManagerService?.CreateTask(this?.Title, this?.Description, this._cancellationToken);
            }
            catch(Exception ex) { this.Logger?.Error($"{this?.Title} - Error in task registration", ex); }
        }

        private void ResolveLogger()
        {
            if(!this.WindsorContainer?.Kernel.HasComponent(typeof(ILogger)) ?? false) return;
            this.Logger = this.WindsorContainer?.Resolve<ILogger>();
        }
    }
}