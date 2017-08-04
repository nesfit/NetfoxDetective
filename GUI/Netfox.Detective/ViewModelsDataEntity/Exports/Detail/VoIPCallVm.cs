using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Detective.Core;
using Netfox.Detective.Core.Interfaces.Model.Exports;
using Netfox.Detective.Core.Interfaces.Views.Exports;
using Netfox.Detective.Views.Exports;
using Netfox.Framework.Snoopers.Models;
using PostSharp.Patterns.Model;
using Telerik.Windows.Controls;
using WPFSoundVisualizationLib;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    public class VoIPCallVm : DetectiveExportDetailPaneViewModelBase
    {

        public VoIPCallVm(WindsorContainer applicationOrAppWindsorContainer, ExportVm model, IVoIPSummary view) : base(applicationOrAppWindsorContainer, model, view)
        {
            this.IsHidden = !this.ExportVm.Calls.Any();
            this.IsActive = this.ExportVm.Calls.Any();

        }
        [IgnoreAutoChangeNotification]
        public IEnumerable<IExportCall> Calls => this.ExportVm.Calls;

        public IExportCall SelectedCall
        {
            get { return this.ExportVm.SelectedSnooperExportObject as IExportCall; }
            set
            {
                var selectedCall = value as SnooperExportedObjectBase;
                if (selectedCall != null) { this.ExportVm.SelectedSnooperExportObject = selectedCall; }
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "VoIPCallView";
        #endregion

    }
}
