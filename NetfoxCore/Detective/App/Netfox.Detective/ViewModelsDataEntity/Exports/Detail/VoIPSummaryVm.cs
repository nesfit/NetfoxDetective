using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    public class VoIPSummaryVm : DetectiveExportDetailPaneViewModelBase
    {
        public VoIPSummaryVm(WindsorContainer applicationWindsorContainer, ExportVm model, IVoIPSummary view) : base(
            applicationWindsorContainer, model, view)
        {
            try
            {
                Task.Run(() =>
                {
                    this.IsHidden = !this.ExportVm.Calls.Any();
                    this.IsActive = this.ExportVm.Calls.Any();
                    this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject,
                        p => this.OnPropertyChanged(nameof(this.SelectedCall)));
                });
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "VoIPSummary";

        #endregion

        [IgnoreAutoChangeNotification]
        public IEnumerable<ICall> Calls => this.ExportVm.Calls.Select(callvm => callvm.Call);

        public ICall SelectedCall
        {
            get { return this.ExportVm.SelectedSnooperExportObject as ICall; }
            set
            {
                var selectedCall = value as SnooperExportedObjectBase;
                if (selectedCall != null)
                {
                    this.ExportVm.SelectedSnooperExportObject = selectedCall;
                }
            }
        }

        [SafeForDependencyAnalysis] public DateTime TimeStampFirst => this.ExportVm.Export.TimeStampFirst;
        [SafeForDependencyAnalysis] public DateTime TimeStampLast => this.ExportVm.Export.TimeStampLast;
        [SafeForDependencyAnalysis] public TimeSpan Duration => this.ExportVm.Duration;
    }
}