using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Castle.Core.Logging;
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces;
using PostSharp.Aspects;

namespace Netfox.Core.Attributes
{
    [Serializable]
    public class AsyncTaskAttribute : OnMethodBoundaryAspect, ILoggable
    {
        [NonSerialized] private CancellationToken _cancellationToken;

        public IWindsorContainer WindsorContainer { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IBgTask BgTaskVm { get; set; }

        public ILogger Logger { get; set; }

        public override void OnEntry(MethodExecutionArgs args)
        {
            var windsorPropertyInfo = args.Instance.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty)
                .FirstOrDefault(i => i.PropertyType.IsAssignableTo(typeof(IWindsorContainer)));
            WindsorContainer = (IWindsorContainer) windsorPropertyInfo?.GetValue(args.Instance);

            this.ResolveLogger();
            this.LogMethodStart();
            this.RegisterWithTaskManager(args);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            this.LogMethodSuccessfulFinish();
            this.FinishTaskInTaskManager();
        }

        private void FinishTaskInTaskManager() { this.BgTaskVm?.Finish(TaskResultState.Ok); }
        
        private void RegisterWithTaskManager(MethodExecutionArgs args)
        {
            if (!this.WindsorContainer?.Kernel.HasComponent(typeof(IBgTasksManagerService)) ?? false) return;

            try
            {
                var bgTasksManagerService = this.WindsorContainer?.Resolve<IBgTasksManagerService>();
                var cancelationToken = args.Arguments.FirstOrDefault(a => a is CancellationToken);
                if (cancelationToken != null) this._cancellationToken = (CancellationToken) cancelationToken;
                this.BgTaskVm =
                    bgTasksManagerService?.CreateTask(this?.Title, this?.Description, this._cancellationToken);
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this?.Title} - Error in task registration", ex);
            }
        }

        private void ResolveLogger()
        {
            if (!this.WindsorContainer?.Kernel.HasComponent(typeof(ILogger)) ?? false) return;
            this.Logger = this.WindsorContainer?.Resolve<ILogger>();
        }

        private void LogMethodStart()
        {
            this.Logger?.Info($"{this?.Title} - Started, {this?.Description}");
        }

        private void LogMethodSuccessfulFinish()
        {
            this.Logger?.Info($"{this?.Title} - Finished, {this?.Description}");
        }
    }
}