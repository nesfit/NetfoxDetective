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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using log4net.Core;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core;
using Netfox.Logger;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Outputs
{
    public sealed class MainOutputVm : DetectiveApplicationPaneViewModelBase
    {
        public MainOutputVm(WindsorContainer applicationWindsorContainer, NetfoxOutputAppender netfoxOutputAppender) : base(applicationWindsorContainer)
        {
            this.NetfoxOutputAppender = netfoxOutputAppender;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IMainOutputView>());
            this.IsHidden = false;
            this.IsSelected = true;
            this.DockPositionPosition = DetectiveDockPosition.DockedBottom;
            this.NetfoxOutputAppender.OutputMessages.CollectionChanged += this.OutputMessagesOnCollectionChanged;
        }

        public override string HeaderText => "Main output";
        public NetfoxOutputAppender NetfoxOutputAppender { get; }

        public LoggingEvent SelectedLogMessage { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CCopyItemToClipBoard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selectedLogMessage = this.SelectedLogMessage;
                    if (selectedLogMessage != null)
                    {
                        Clipboard.SetData(DataFormats.Text, selectedLogMessage.ToString());
                    }
                });
            }
        }

        [SafeForDependencyAnalysis]
        public ObservableCollection<LoggingEvent> OutputMessages => this.NetfoxOutputAppender.OutputMessages;

        private void OutputMessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args?.NewItems == null || args.NewItems.Count == 0)
            {
                return;
            }

            var sysmsg = args.NewItems[0] as LoggingEvent;
            this.SelectedLogMessage = sysmsg;
        }
    }
}