// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.Snoopers.SnooperMAFF.Interfaces;
using Netfox.Snoopers.SnooperMAFF.Models.Exports;
using Netfox.Snoopers.SnooperMAFF.WPF.View.Events;

namespace Netfox.Snoopers.SnooperMAFF.WPF.ViewModels
{
    /// <summary>
    /// Class holds Visualisation Model used by binding in ViewVisualization Class.
    /// Class loads Archive used by CefSharp for Page visualization. 
    /// </summary>
    /// <seealso cref="DetectiveExportDetailPaneViewModelBase" />
    public class SnooperMAFFViewVisualizationModel : DetectiveExportDetailPaneViewModelBase
    {
        public int CurrentPart { get; set; }
        public int PartCount { get; set; }
        public SnooperExportedArchive ExportedArchive { get; set; }
        public override string HeaderText => "Archive visualization";

        public string GetPathToArchive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnooperMAFFViewVisualizationModel"/> class.
        /// </summary>
        /// <param name="applicationOrAppWindsorContainer">The application or application windsor container.</param>
        /// <param name="model">The model holds exported objects from snoopers.</param>
        /// <param name="view">The view of Archive Visualization.</param>
        public SnooperMAFFViewVisualizationModel(IWindsorContainer applicationOrAppWindsorContainer, ExportVm model, IMAFFExportsViewVisualization view) : base(applicationOrAppWindsorContainer, model, view)
        {
            try
            {
                Task.Run(() =>
                {
                    var tmpExports = model.SnooperExportedObjects.Where(i => i is SnooperExportedArchive).Cast<SnooperExportedArchive>().ToArray();
                    if (tmpExports.Length > 0)
                    {
                        this.ExportedArchive = tmpExports[0];
                        this.PartCount = this.ExportedArchive.Archive.ListOfArchiveParts.Count;
                        CurrentPartEvent.GetInstance().Inicialize(this, this.PartCount);
                        this.CallEventHandler();

                        this.IsActive = (this.PartCount > 0);
                        this.IsHidden = !(this.PartCount > 0);
                    }
                });
            }
            catch (Exception ex)
            {
                this.Logger.Error($"{this.GetType().Name} MAFFVm instantiation failed", ex);
            }
        }

        /// <summary>
        /// Calls the event handler when Current part of archive was changed.
        /// </summary>
        public void CallEventHandler()
        {
            this.OnPropertyChanged(nameof(this.GetPathToArchive));
            this.OnPropertyChanged(nameof(this.CurrentPart));
            this.RaisePropertyChanged();
        }
    }
}
