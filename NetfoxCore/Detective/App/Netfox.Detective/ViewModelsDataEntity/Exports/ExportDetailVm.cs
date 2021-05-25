using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModels;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    public class ExportDetailVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ExportDetailVm(WindsorContainer applicationWindsorContainer, ExportVm model) : base(
            applicationWindsorContainer, model)
        {
            this.ExportVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IExportDetailView>());
            this.ScanAssembliesForExportDetailPanesAndRegisterThem();
        }

        #region Overrides of DetectivePaneViewModelBase

        public override String HeaderText => this.ExportVm.Name + "Export detail";

        #endregion

        public ConcurrentObservableCollection<DetectivePaneViewModelBase> ViewPanesVMs { get; } =
            new ConcurrentObservableCollection<DetectivePaneViewModelBase>();

        public ExportVm ExportVm { get; }

        private void ScanAssembliesForExportDetailPanesAndRegisterThem()
        {
            foreach (var currentAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Type> currentAssemblyTypes;
                try
                {
                    currentAssemblyTypes = currentAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    this.ApplicationOrInvestigationWindsorContainer.Resolve<ILogger>()
                        .Error(
                            $"Current assembly contains one or more types that cannot be loaded.\ncurrentAssembly={currentAssembly},\nException: {e}");
                    // use at least those types that were loaded
                    currentAssemblyTypes = e.Types.Where(a => a != null);
                }

                var exportDetailVmTypes = currentAssemblyTypes.Where(a =>
                    a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract &&
                    typeof(DetectiveExportDetailPaneViewModelBase).IsAssignableFrom(a));

                foreach (var vmType in exportDetailVmTypes)
                    try
                    {
                        if (!this.ApplicationOrInvestigationWindsorContainer.Kernel.HasComponent(vmType))
                            try
                            {
                                this.ApplicationOrInvestigationWindsorContainer.Register(Component.For(vmType)
                                    .LifestyleTransient());
                            }
                            catch (Exception ex)
                            {
                                this.Logger?.Error($"etail export Vm is possibly already registered {vmType.Name}", ex);
                            }

                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            try
                            {
                                var vm = this.ApplicationOrInvestigationWindsorContainer.Resolve(vmType, new Dictionary<string, object>
                                {
                                    {"model", this.ExportVm},
                                    {"investigationOrAppWindsorContainer", this.ApplicationOrInvestigationWindsorContainer}
                                });
                                ;
                                Debug.Assert(vm != null, "viewModel != null");
                                this.ViewPanesVMs.Add(vm as DetectivePaneViewModelBase);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error($"Export detail instantiation failed {vmType.Name}", ex);
                    }
            }
        }
    }
}