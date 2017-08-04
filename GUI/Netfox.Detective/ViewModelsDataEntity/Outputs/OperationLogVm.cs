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

using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core;
using Netfox.Detective.Models;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Outputs
{
    public class OperationLogVm : DetectiveIvestigationDataEntityPaneViewModelBase, IDataEntityVm
    {
        public OperationLogVm(WindsorContainer applicationWindsorContainer, OperationLog model) : base(applicationWindsorContainer, model)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IOperationLogView>());
            this.OperationLog = model;
            this.DockPositionPosition = DetectiveDockPosition.DockedBottom;
        }

        [SafeForDependencyAnalysis]
        public override string HeaderText => this.OperationLog.Name;

        public OperationLog OperationLog { get; set; }
    }
}