using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Views;

namespace Netfox.Detective.ViewModels.Exports
{
    public class ExportContentExplorerVm: DetectiveApplicationPaneViewModelBase
    {
        public ExportContentExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IExportContentExplorerView>());
            Parallel.Invoke(this.ScanAssembliesForeExportPaneViewAndRegisterThem);
        }

        public void RegisterDetectivePaneView(DetectiveExportDetailPaneViewBase detectiveApplicationDetailPaneView)
        {
            var viewModel = detectiveApplicationDetailPaneView.DataContext as DetectiveApplicationPaneViewModelBase;
            Debug.Assert(viewModel != null, "viewModel != null");
            viewModel.View = detectiveApplicationDetailPaneView;
            this.ExportPanesVMs.Add(viewModel);
        }

        private void ScanAssembliesForeExportPaneViewAndRegisterThem()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var viewsTypes =
                currentAssembly.GetTypes()
                    .Where(
                        a =>
                            a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract && a.GetConstructor(Type.EmptyTypes) != null
                            && typeof(DetectiveExportDetailPaneViewBase).IsAssignableFrom(a));

            foreach(var viewType in viewsTypes)
            {
                var type = viewType;
                DispatcherHelper.RunAsync(() =>
                {
                    var detectiveView = (DetectiveExportDetailPaneViewBase) Activator.CreateInstance(type);
                    //skip DataEntityPaneViewModels
                    if(detectiveView.DataContext != null) { this.RegisterDetectivePaneView(detectiveView); }
                });
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Export content explorer";
        public ConcurrentObservableCollection<DetectivePaneViewModelBase> ExportPanesVMs { get; } = new ConcurrentObservableCollection<DetectivePaneViewModelBase>();
        #endregion


    }
}