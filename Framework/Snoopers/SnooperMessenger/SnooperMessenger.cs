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
using System.Collections.Generic;
using System.IO;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperMessenger.Models;
using Netfox.SnooperMessenger.Models.Events;
using Netfox.SnooperMessenger.Protocol;
using Netfox.SnooperMQTT.Models;
using Netfox.SnooperMQTT.Models.Commands;

namespace Netfox.SnooperMessenger
{
    public class SnooperMessenger : SnooperBase
    {
        public SnooperMessenger() { }

        public SnooperMessenger(
            WindsorContainer investigationWindsorContainer,
            IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory) : base(investigationWindsorContainer, sourceExports, exportDirectory) {}

        public override string Name => "iOS Messenger";

        public override string[] ProtocolNBARName => new[]
        {
            "mqtt",
            "ssl",
            "messenger"
        };

        public override string Description => "iOS Messenger Snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new MessengerSnooperExport();
        protected override SnooperExportBase CreateSnooperExport() { return new MessengerSnooperExport(); }

        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();
            this.ProcessConversation();
            this.OnConversationProcessingEnd();
        }

        protected override void ProcessConversation()
        {
            foreach(var snooperExportBase in this.SourceExports)
            {
                foreach(SnooperMQTTExportedObject exportedObject in snooperExportBase.ExportObjects)
                {
                    this.OnBeforeProtocolParsing();

                    MessengerEventBase messengerEvent = null;

                    // Process MQTT publish messages
                    var mqttMessage = exportedObject as MQTTMsg;
                    var mqttCommand = mqttMessage?.Command;
                    if (mqttCommand is MQTTCommandPublish)
                    {
                        var mqttPublishCommand = mqttCommand as MQTTCommandPublish;
                        switch (mqttPublishCommand.Topic)
                        {
                            case "/foreground_state":
                                messengerEvent = this.HandleEvent<MessengerEventForegroundState>(this.SnooperExport, exportedObject.ExportSource, mqttPublishCommand.Payload);
                                break;
                            case "/typing":
                                messengerEvent = this.HandleEvent<MessengerEventTyping>(this.SnooperExport, exportedObject.ExportSource, mqttPublishCommand.Payload);
                                break;
                            case "/t_sm":
                                messengerEvent = this.HandleEvent<MessengerEventSendMessage>(this.SnooperExport, exportedObject.ExportSource, mqttPublishCommand.Payload);
                                break;
                            case "/t_ms":
                                var syncClientPayload = new MNMessagesSyncClientPayload();
                                syncClientPayload.Read(MessengerEventBase.CompactProtocolForPayload(MessengerEventBase.DecompressPayload(mqttPublishCommand.Payload)));
                                this.HandleSyncMessage(this.SnooperExport, exportedObject.ExportSource, syncClientPayload, mqttMessage.TimeStamp, mqttMessage.Frames);
                                break;
                            default:
                                continue;
                        }
                    }
                    else if(mqttCommand is MQTTCommandConnect)
                    {
                        var mqttConnectCommand = mqttCommand as MQTTCommandConnect;
                        messengerEvent = this.HandleEvent<MessengerEventConnect>(this.SnooperExport, exportedObject.ExportSource, mqttConnectCommand.Payload);
                    }
                    else continue;

                    if (messengerEvent == null)
                        continue;
                   
                    messengerEvent.TimeStamp = mqttMessage.TimeStamp;
                    messengerEvent.Frames.AddRange(mqttMessage.Frames);
                }
            }
        }

        // Handle event of given event class
        protected MessengerEventBase HandleEvent<THangoutsEventClass>(SnooperExportBase exportObject, IExportSource exportSource, byte[] payload)
        {
            if (!typeof(THangoutsEventClass).IsSubclassOf(typeof(MessengerEventBase)))
                throw new Exception("Not a MessengerEventBase subclass");

            // try, except create report
            var messengerEvent = (MessengerEventBase)Activator.CreateInstance(typeof(THangoutsEventClass), new object[] { exportObject, payload });

            this.OnAfterProtocolParsing();

            this.OnBeforeDataExporting();
            messengerEvent.ExportSources.Add(exportSource);
            this.SnooperExport.AddExportObject(messengerEvent);
            this.OnAfterDataExporting();

            return messengerEvent;
        }

        protected void HandleSyncMessage(SnooperExportBase exportObject, IExportSource exportSource, MNMessagesSyncClientPayload syncClientPayload, DateTime timestamp, IEnumerable<PmFrameBase> frames)
        {
            foreach(var syncDeltaWrapper in syncClientPayload.Deltas)
            {
                MessengerEventBase deltaEvent = null;
                if (syncDeltaWrapper.DeltaNewMessage != null)
                    deltaEvent = new MessengerEventReceiveMessage(exportObject, syncDeltaWrapper.DeltaNewMessage);
                else
                    continue;

                this.OnAfterProtocolParsing();

                deltaEvent.TimeStamp = timestamp;
                deltaEvent.Frames.AddRange(frames);

                this.OnBeforeDataExporting();
                deltaEvent.ExportSources.Add(exportSource);
                this.SnooperExport.AddExportObject(deltaEvent);
                this.OnAfterDataExporting();
            }
        }
    }
}