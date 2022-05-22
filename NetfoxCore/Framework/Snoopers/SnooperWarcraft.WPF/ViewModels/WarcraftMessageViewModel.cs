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
using Netfox.Snoopers.SnooperWarcraft.Interfaces;
using Netfox.Snoopers.SnooperWarcraft.Models;
using PostSharp.Patterns.Model;

namespace Netfox.Snoopers.SnooperWarcraft.WPF.ViewModels
{
    [NotifyPropertyChanged]
    public class WarcraftMessageViewModel : DetectiveExportDetailPaneViewModelBase
    {
        private string _filter = "";
        private ObservableCollection<SnooperExportedWarcraftMessage> _messages;

        public ObservableCollection<SnooperExportedWarcraftMessage> Messages
        {
            get { return this._messages; }
            private set
            {
                this._messages = value;
                this.OnPropertyChanged(nameof(this.FilteredMessages));
            }
        }

        public SnooperExportedWarcraftMessage SelectedWarcraftMessage
        {
            get { return this.ExportVm.SelectedSnooperExportObject as SnooperExportedWarcraftMessage; }
            set { this.ExportVm.SelectedSnooperExportObject = value; }
        }

        [IgnoreAutoChangeNotification]
        public ObservableCollection<SnooperExportedWarcraftMessage> FilteredMessages
        {
            get
            {
                var oc = new ObservableCollection<SnooperExportedWarcraftMessage>();
                foreach (var msg in this.Messages)
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
                if (value == this._filter) { return; }
                this._filter = value;
                this.OnPropertyChanged(nameof(this.FilteredMessages));
            }
        }

        public WarcraftMessageViewModel(WindsorContainer applicationWindsorContainer, ExportVm model, IWarcraftConversationView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                    this.Messages = new ObservableCollection<SnooperExportedWarcraftMessage>(
                            model.SnooperExportedObjects.Where(x => x is SnooperExportedWarcraftMessage).Cast<SnooperExportedWarcraftMessage>());
                    this.IsHidden = !this.Messages.Any();
                    this.IsActive = this.Messages.Any();
                    this.ExportVmObserver.RegisterHandler(x => x.SelectedSnooperExportObject, x => this.OnPropertyChanged(nameof(this.SelectedWarcraftMessage)));
            }
            catch (Exception ex) {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "WarcraftMessage";
        #endregion

    }
}
