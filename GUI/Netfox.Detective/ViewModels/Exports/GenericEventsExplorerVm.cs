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

using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Core.Messages.Exports;
using Netfox.Detective.Core;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports
{
    public class GenericEventsExplorerVm : DetectiveApplicationPaneViewModelBase
    {
        public GenericEventsExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.DockPositionPosition = DetectiveDockPosition.DockedLeft;
            Parallel.Invoke(() => Messenger.Default.Register<ExportResultMessage>(this, this.ExportResultActionHandler));
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "All events";
        #endregion

        public ExportVm ExportResult { get; set; }

        private void ExportResultActionHandler(ExportResultMessage exportResultMessage) { this.ExportResult = exportResultMessage.ExportVm as ExportVm; }
    }
}