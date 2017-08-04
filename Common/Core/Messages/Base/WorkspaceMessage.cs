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

namespace Netfox.Core.Messages.Base
{
    /// <summary>
    ///     Detective message class. Informs about creation or selection of application session.
    /// </summary>
    public class WorkspaceMessage : DetectiveMessage
    {
        public enum Type
        {
            Created,
            ToCreate,
            Opened,
            Closed,
            Opening,
            Closing
        }

        public WorkspaceMessage() { this.Sender = "WorkspaceMessage"; }
        public object Workspace { get; set; }
        public Type MessageType { get; set; }

        public static void SendWorkspaceMessage(object workspace, Type messageType, SendMethod sendMethod = SendMethod.Async)
        {
            var message = new WorkspaceMessage
            {
                Workspace = workspace,
                MessageType = messageType
            };
            switch(sendMethod)
            {
                case SendMethod.Async:
                    AsyncSendMessage(message);
                    break;
                case SendMethod.Blocking:
                    BlockingSendMessage(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sendMethod");
            }
        }
    }
}