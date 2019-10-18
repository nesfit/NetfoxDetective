// Copyright (c) 2017 Jan Pluskal
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

using AlphaChiTech.VirtualizingCollection.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IConversationsModel
    {
        //ICollection<ILxConversation> Conversations { get; }
        IObservableCollection<PmFrameBase> Frames { get; }
        IObservableCollection<L3Conversation> L3Conversations { get; }
        IObservableCollection<L4Conversation> L4Conversations { get; }
        IObservableCollection<L7Conversation> L7Conversations { get; }
        //ConversationsSummaryStatistics Statistics { get; }
        //IObservableCollection<PmFrameBase> Frames { get; }
        IObservableCollection<SnooperExportBase> SnooperExports { get; }
        IObservableCollection<ISnooper> UsedSnoopers { get; }
    }
}
