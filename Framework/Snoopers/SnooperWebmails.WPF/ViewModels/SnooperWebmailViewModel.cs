// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.SnooperWebmails.Interfaces;
using Netfox.SnooperWebmails.Models.WebmailEvents;
using PostSharp.Patterns.Model;

namespace Netfox.SnooperWebmails.WPF.ViewModels
{
    public class SnooperWebmailViewModel : DetectiveExportDetailPaneViewModelBase
    {

        public WebmailEventBase SelectedEvent
        {
            get { return this.ExportVm.SelectedSnooperExportObject as WebmailEventBase; }
            set
            {
                this.ExportVm.SelectedSnooperExportObject = value;
                this.OnPropertyChanged(nameof(this.MessageList));
            }
        }

        [IgnoreAutoChangeNotification]
        public List<MailMsg> MessageList
        {
            get
            {
                if(this.SelectedEvent is EventListFolder)
                {
                    var e = this.SelectedEvent as EventListFolder;
                    this.OnPropertyChanged(nameof(this.SelectedMsg));
                    return e?.Messages;
                }
                else if(this.SelectedEvent is EventDisplayMessage)
                {
                    var e = this.SelectedEvent as EventDisplayMessage;
                    this.OnPropertyChanged(nameof(this.SelectedMsg));
                    return new List<MailMsg>() {e?.Mail};
                }
                else if (this.SelectedEvent is EventNewMessage)
                {
                    var e = this.SelectedEvent as EventNewMessage;
                    this.OnPropertyChanged(nameof(this.SelectedMsg));
                    return new List<MailMsg>() { e?.Mail };
                }
                return null;
            }
        }

        private MailMsg _selectedMsg;

        public MailMsg SelectedMsg
        {
            get { return this._selectedMsg; }
            set
            {
                this._selectedMsg = value;
                this.OnPropertyChanged();
            }
        }

        public IEnumerable<WebmailEventBase> WebmailEvents { get; set; }

        public SnooperWebmailViewModel(WindsorContainer applicationWindsorContainer, ExportVm model, IWebmailExportsView view) : base(applicationWindsorContainer, model, view)
        {
            try
            {
                    this.WebmailEvents = model.SnooperExportedObjects.Where(x => x is WebmailEventBase).Cast<WebmailEventBase>().ToArray();
                    this.IsHidden = !this.WebmailEvents.Any();
                    this.IsActive = this.WebmailEvents.Any();
                    this.ExportVmObserver.RegisterHandler(x => x.SelectedSnooperExportObject, x => this.OnPropertyChanged(nameof(this.SelectedEvent)));
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
            
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Snooper Webmails";
        #endregion
    }
}
