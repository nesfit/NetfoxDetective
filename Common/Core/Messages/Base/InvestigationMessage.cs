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
using System.Diagnostics;

namespace Netfox.Core.Messages.Base
{
    public class InvestigationMessage : DetectiveMessage
    {
        public enum MessageType
        {
            InvestigationCreated,
            CurrentInvestigationChanged
        };

        private object _investigation;
        private InvestigationMessage() { this.Sender = "InvestigationMessage"; }
        public object InvestigationVm { get; set; }

        public object Investigation
        {
            get
            {
                if(this._investigation != null) { return this._investigation; }
                if(this.InvestigationVm == null) { return null; }
                try
                {
                    dynamic investigationVm = this.InvestigationVm;
                    return investigationVm.Investigation;
                }
                catch(Exception)
                {
                    Debugger.Break(); // investigationVm is not instance of investigationVm instance
                    return null;
                }
            }
            set { this._investigation = value; }
        }

        public MessageType Type { get; set; }

        public static void SendInvestigationMessage(object investigationOrInvestigationVm, MessageType type)
        {
            var newInvestigationMessage = new InvestigationMessage
            {
                Type = type
            };
            switch(type)
            {
                case MessageType.InvestigationCreated:
                    newInvestigationMessage.InvestigationVm = investigationOrInvestigationVm;
                    break;
                case MessageType.CurrentInvestigationChanged:
                    newInvestigationMessage.InvestigationVm = investigationOrInvestigationVm;
                    break;
            }

            AsyncSendMessage(newInvestigationMessage);
        }
    }
}