// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperMessenger.Protocol;

namespace Netfox.SnooperMessenger.Models.Events
{
    public class MessengerEventSendMessage : MessengerEventBase
    {
        public string To { get; private set; }
        public string Body { get; private set; }
        public string Attachement { get; private set; }
        public string LocationAttachement { get; private set; }

        private MessengerEventSendMessage() { } //EF
        public MessengerEventSendMessage(SnooperExportBase exportBase, Byte[] payload) : base(exportBase)
        {
            var sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.Read(CompactProtocolForPayload(DecompressPayload(payload)));

            this.To = sendMessageRequest.To;
            this.Body = sendMessageRequest.Body;
            this.Attachement = sendMessageRequest.ObjectAttachement;

            var locationAttachementCoordinates = sendMessageRequest.LocationAttachement?.Coordinates;
            if(locationAttachementCoordinates != null)
            {
                this.LocationAttachement = locationAttachementCoordinates.Longitude + " " + locationAttachementCoordinates.Latitude;
            }
           
        }

        public override string ToString()
        {
            return "To:" + this.To + " Body:" + this.Body + " Attachement:" + this.Attachement + "LocationAttachement:" + this.LocationAttachement;
        }
    }
}