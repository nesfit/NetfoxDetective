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

using System.Collections.Generic;
using System.Text;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ReassembledStreamDetailVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ReassembledStreamDetailVm(WindsorContainer applicationWindsorContainer, ConversationVm model) : base(applicationWindsorContainer, model)
        {
            this.ConversationVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IReassembledStreamDetailView>());
        }

        #region Overrides of DetectivePaneViewModelBase
        [SafeForDependencyAnalysis]
        public override string HeaderText => "Reassembled stream " + this.ConversationVm.Conversation.Name;
        #endregion

        public ConversationVm ConversationVm { get; }

        public IEnumerable<EncodingInfo> Encodings => Encoding.GetEncodings();
    }
}