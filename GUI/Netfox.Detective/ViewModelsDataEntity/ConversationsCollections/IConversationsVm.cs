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

using System.Collections;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.ViewModelsDataEntity.ConversationsCollections
{
    public interface IConversationsVm : INotifyPropertyChanged
    {
        ViewModelVirtualizingIoCObservableCollection<FrameVm, PmFrameBase> Frames { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L3Conversation> L3Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L4Conversation> L4Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<ConversationVm, L7Conversation> L7Conversations { get; }
        ViewModelVirtualizingIoCObservableCollection<SnooperVm, ISnooper> UsedSnoopers { get; }
        ConversationVm CurrentConversation { get; set; }
        //ConversationVm[] ConversationsByTraffic { get; }
        //ConversationVm[] ConversationsByDuration { get; }
        ExportVm[] AllExportResults { get; }
        RelayCommand CAddConvGroupToExportClick { get; }
        ConversationsStatisticsVm ConversationsStatisticsVm { get; }
        bool IsSelected { get; set; }
        RelayCommand<ConversationVm> CShowConversationDetail { get; }
        RelayCommand<IList> CCreateConversationsGroup { get; }
        RelayCommand<FrameVm> CShowFrameDetail { get; }
        NotifyTaskCompletion<ConversationVm[]> ConversationsByTraffic { get; }
        NotifyTaskCompletion<ConversationVm[]> ConversationsByDuration { get; }
        ConversationVm SetCurrentConversationByFrame(FrameVm frameVm);
    }
}