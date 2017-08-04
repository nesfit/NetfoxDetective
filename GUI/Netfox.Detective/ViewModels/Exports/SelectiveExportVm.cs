// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Messages.Base;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Exports
{
    [NotifyPropertyChanged]
    public class SelectiveExportVm : DetectiveApplicationPaneViewModelBase
    {
        public SelectiveExportVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<ISelectiveExportView>());
            Task.Factory.StartNew(() => Messenger.Default.Register<InvestigationMessage>(this, this.InvestigationActionHandler));
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Selective export";
        #endregion
        [DoNotWire]
        public InvestigationVm InvestigationVm { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CExport => null;

        //this._CExport  ?? (this._CExport = new RelayCommand(() => this.InvestigationVm.ExportExecute(), () => this.InvestigationVm != null && this.InvestigationVm.ToExportConversations.Count > 0));

        [IgnoreAutoChangeNotification]
        public ICommand CClearToExport
        {
            get
            {
                //return new RelayCommand(() => ToExportConversations.Clear(), () => ToExportConversations.Count > 0);
                return new RelayCommand(() => this.InvestigationVm.ToExportConversations.Clear());
            }
        }

        private void InvestigationActionHandler(InvestigationMessage message)
        {
            if(message.Type != InvestigationMessage.MessageType.CurrentInvestigationChanged) { return; }
            this.InvestigationVm = message.InvestigationVm as InvestigationVm;

            if(this.InvestigationVm == null) { return; }
            this.InvestigationVm.ToExportConversations.CollectionChanged += this.ToExportConversationsOnCollectionChanged;
        }

        [IgnoreAutoChangeNotification]
        private void ToExportConversationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            DispatcherHelper.RunAsync(() => this.CExport.RaiseCanExecuteChanged());
        }
    }
}