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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperMAFF.Interfaces;
using Netfox.SnooperMAFF.Models.Exports;
using Netfox.SnooperMAFF.Models.Objects;
using Netfox.SnooperMAFF.View.Events;

namespace Netfox.SnooperMAFF.ViewModels
{
    /// <summary>
    /// Class holds Visualisation Model used by binding in View model Class.
    /// Class loads Archive details content and show in View. 
    /// </summary>
    /// <seealso cref="DetectiveExportDetailPaneViewModelBase" />
    public class SnooperMAFFViewModel : DetectiveExportDetailPaneViewModelBase
    {
        public SnooperExportedArchive ExportedArchive;
        public IEnumerable<BaseObject> ArchiveObjects { get; set; }
        public int CurrentPart { get; set; }
        public int PartCount { get; set; }
        private BaseObject _selectedObject;

        /// <summary>
        /// Gets the information about selected object in View.
        /// </summary>
        /// <value>
        /// The selected object.
        /// </value>
        public BaseObject SelectedObject
        {
            get { return this._selectedObject; }
            set
            {
                this._selectedObject = value;
                this.OnPropertyChanged(nameof(this.ObjectContent));
                this.OnPropertyChanged(nameof(this.ObjectReferences));
            }
        }

        /// <summary>
        /// Gets the name of the selected object.
        /// </summary>
        /// <value>
        /// The name of the object.
        /// </value>
        public string ObjectName
        {
            get
            {
                if (this._selectedObject == null) return string.Empty;
                return this._selectedObject.FileName;
            }
        }

        /// <summary>
        /// Gets the text object references.
        /// </summary>
        /// <value>
        /// The object references of text object.
        /// </value>
        public List<string> ObjectReferences
        {
            get
            {
                if (this._selectedObject == null) return new List<string>();
                return this._selectedObject.ListOfNewReferences;
            }
        }

        /// <summary>
        /// Gets the content of the text object.
        /// </summary>
        /// <value>
        /// The content of the object.
        /// </value>
        public string ObjectContent
        {
            get
            {
                if (this._selectedObject == null) { return string.Empty; }
                if (this._selectedObject is TextObject)
                {
                    return this._selectedObject.ToString();
                }
                return string.Empty;
            }
        }

        public override string HeaderText => "Archive detail";

        /// <summary>
        /// Initializes a new instance of the <see cref="SnooperMAFFViewModel"/> class.
        /// </summary>
        /// <param name="applicationOrAppWindsorContainer">The application or application windsor container.</param>
        /// <param name="model">The model holds exported objects from snoopers.</param>
        /// <param name="view">The view of Archive Visualization.</param>
        public SnooperMAFFViewModel(IWindsorContainer applicationOrAppWindsorContainer, ExportVm model, IMAFFExportsView view) : base(applicationOrAppWindsorContainer, model, view)
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

        public void CallEventHandler()
        {
            this.OnPropertyChanged(nameof(this.CurrentPart));
            this.RaisePropertyChanged();
        }
    }
}