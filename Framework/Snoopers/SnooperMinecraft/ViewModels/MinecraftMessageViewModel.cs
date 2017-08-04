// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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
using Netfox.SnooperMinecraft.Interfaces;
using Netfox.SnooperMinecraft.Models;
using PostSharp.Patterns.Model;

namespace Netfox.SnooperMinecraft.ViewModels
{
    [NotifyPropertyChanged]
    public class MinecraftMessageViewModel : DetectiveExportDetailPaneViewModelBase
    {
        private string _filter = "";
        private ObservableCollection<SnooperExportedMinecraftMessage> _minecraftMessages;

        public ObservableCollection<SnooperExportedMinecraftMessage> MinecraftMessages
        {
            get { return this._minecraftMessages; }
            private set
            {
                this._minecraftMessages = value; 
                this.OnPropertyChanged(nameof(this.FilteredMessages));
            }
        }

        public SnooperExportedMinecraftMessage SelectedMinecraftMessage
        {
            get { return this.ExportVm.SelectedSnooperExportObject as SnooperExportedMinecraftMessage; }
            set { this.ExportVm.SelectedSnooperExportObject = value; }
        }

        [IgnoreAutoChangeNotification]
        public ObservableCollection<SnooperExportedMinecraftMessage> FilteredMessages
        {
            get
            {
                var oc = new ObservableCollection<SnooperExportedMinecraftMessage>();
                foreach (var msg in this.MinecraftMessages)
                {
                    if (msg.Message.Contains(this.Filter))
                        oc.Add(msg);
                    else if (msg.Receiver.Contains(this.Filter))
                        oc.Add(msg);
                    else if (msg.Sender.Contains(this.Filter))
                        oc.Add(msg);
                    else if (msg.Type.ToString().Contains(this.Filter))
                        oc.Add(msg);
                    else if (msg.TimeStamp.ToString().Contains(this.Filter))
                        oc.Add(msg);
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
                this.OnPropertyChanged(nameof(this.FilteredMessages));
            }
        }

        public MinecraftMessageViewModel(WindsorContainer applicationWindsorContainer, ExportVm model, IMinecraftMsgView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                    this.MinecraftMessages = new ObservableCollection<SnooperExportedMinecraftMessage>(
                            model.SnooperExportedObjects.Where(x => x is SnooperExportedMinecraftMessage).Cast<SnooperExportedMinecraftMessage>());
                    this.IsHidden = !this.MinecraftMessages.Any();
                    this.IsActive = this.MinecraftMessages.Any();
                    this.ExportVmObserver.RegisterHandler(x => x.SelectedSnooperExportObject, x => this.OnPropertyChanged(nameof(this.SelectedMinecraftMessage)));
            }
            catch(Exception ex) {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Minecraft detail";
        #endregion
    }
}