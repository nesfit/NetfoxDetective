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

using System.Linq;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Framework.Models.PmLib.Frames;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Frames
{
    public class FrameContentVm : DetectiveIvestigationDataEntityPaneViewModelBase, IFrameContentView
    {
        public FrameContentVm(WindsorContainer applicationWindsorContainer, FrameVm model) : base(applicationWindsorContainer, model)
        {
            this.FrameVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IFrameContentView>());
        }

        #region Overrides of DetectivePaneViewModelBase
        [SafeForDependencyAnalysis]
        public override string HeaderText => "Frame content: " + this.FrameVm.Frame.SourceEndPoint + " - " + this.FrameVm.Frame.DestinationEndPoint;
        #endregion

        public FrameVm FrameVm { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CNextFrame => new RelayCommand(() =>
        {
            if(this.FrameVm == null) { return; }
            this.FrameVm.ApplicationOrInvestigationWindsorContainer.Release(this);
            var frameModel =
                this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>()
                    .FirstOrDefault(f => f.FrameIndex == this.FrameVm.FrameIndex + 1 && f.PmCapture == this.FrameVm.FwFrame.PmCapture);
            if(frameModel == null) { return; }
            this.FrameVm = this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new
            {
                model = frameModel,
                investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
        });

        [IgnoreAutoChangeNotification]
        public RelayCommand CPreviousFrame => new RelayCommand(() =>
        {
            if(this.FrameVm == null) { return; }
            this.FrameVm.ApplicationOrInvestigationWindsorContainer.Release(this);
            var frameModel =
                this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>()
                    .FirstOrDefault(f => f.FrameIndex == this.FrameVm.FrameIndex - 1 && f.PmCapture == this.FrameVm.FwFrame.PmCapture);
            if(frameModel == null) { return; }
            this.FrameVm = this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new
            {
                model = frameModel,
                investigationOrAppWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
        });
    }
}