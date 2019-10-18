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

using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IConversationsModel
    {
        //ICollection<ILxConversation> Conversations { get; }
        ICollection<PmFrameBase> Frames { get; }
        ICollection<L3Conversation> L3Conversations { get; }
        ICollection<L4Conversation> L4Conversations { get; }
        ICollection<L7Conversation> L7Conversations { get; }
        //ConversationsSummaryStatistics Statistics { get; }
        //IObservableCollection<PmFrameBase> Frames { get; }
        ICollection<SnooperExportBase> SnooperExports { get; }
        ICollection<ISnooper> UsedSnoopers { get; }
    }
}
