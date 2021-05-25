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

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Database;

namespace Netfox.Persistence.JunctionTypes
{
    [Serializable]
    public abstract class PmCaptureConversation
    {
        protected PmCaptureConversation() { }

        protected PmCaptureConversation(Guid captureId, Guid conversationId)
        {
            this.PmCaptureId = captureId;
            this.ConversationId = conversationId;
        }

        protected PmCaptureConversation(IEntity capture, IEntity conversation)
        {
            this.PmCaptureId = capture.Id;
            this.ConversationId = conversation.Id;
        }

        [NotMapped]
        public Guid PmCaptureId { get; set; }

        [NotMapped]
        public Guid ConversationId { get; set; }
    }
}