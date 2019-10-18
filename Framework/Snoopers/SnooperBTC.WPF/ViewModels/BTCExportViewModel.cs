// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
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
using System.Collections.ObjectModel;
using System.Linq;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperBTC.Interfaces;
using Netfox.SnooperBTC.Models;
using PostSharp.Patterns.Model;

namespace Netfox.SnooperBTC.WPF.ViewModels
{
    [NotifyPropertyChanged]
    public class BTCExportViewModel : DetectiveExportDetailPaneViewModelBase
    {
        private string _filter = "";
        private ObservableCollection<SnooperExportedDataObjectBTC> _btcObjects;

        public ObservableCollection<SnooperExportedDataObjectBTC> BTCObjects
        {
            get { return this._btcObjects; }
            private set
            {
                this._btcObjects = value; 
                this.OnPropertyChanged(nameof(this.FilteredObjects));
            }
        }

        public SnooperExportedDataObjectBTC SelectedBTCObject
        {
            get { return this.ExportVm.SelectedSnooperExportObject as SnooperExportedDataObjectBTC; }
            set { this.ExportVm.SelectedSnooperExportObject = value; }
        }

        [IgnoreAutoChangeNotification]
        public ObservableCollection<SnooperExportedDataObjectBTC> FilteredObjects
        {
            get
            {
                var oc = new ObservableCollection<SnooperExportedDataObjectBTC>();
                foreach (var obj in this.BTCObjects)
                {
                    //if (msg.Message.Contains(this.Filter))
                    //    oc.Add(msg);
                    //else if (msg.Receiver.Contains(this.Filter))
                    //    oc.Add(msg);
                    //else if (msg.Sender.Contains(this.Filter))
                    //    oc.Add(msg);
                    //else if (msg.Type.ToString().Contains(this.Filter))
                    //    oc.Add(msg);
                    //else if (msg.TimeStamp.ToString().Contains(this.Filter))
                    //    oc.Add(msg);
                    oc.Add(obj);
                }
                return oc;
            }
        }

        public string Filter
        {
            get { return this._filter; }
            set
            {
                if(value == this._filter) { return; }
                this._filter = value;
                this.OnPropertyChanged(nameof(this.FilteredObjects));
            }
        }

        public BTCExportViewModel(WindsorContainer applicationWindsorContainer, ExportVm model, IBTCExportView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                    this.BTCObjects = new ObservableCollection<SnooperExportedDataObjectBTC>(
                            model.SnooperExportedObjects.Where(x => x is SnooperExportedDataObjectBTC).Cast<SnooperExportedDataObjectBTC>());
                    this.IsHidden = !this.BTCObjects.Any();
                    this.IsActive = this.BTCObjects.Any();

            }
            catch(Exception ex) {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Bitcoin detail";
        #endregion
    }
}