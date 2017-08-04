// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.SnooperWarcraft.Models
{
    public class SnooperExportedWarcraftMessage : SnooperExportedObjectBase, IChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty; // complete unparsed message
        public WarcraftMessageType Type { get; set; } = WarcraftMessageType.Say; // say is default and used mainly

        public System.Windows.Media.SolidColorBrush TypeColor
        {
            get
            {
                switch (this.Type)
                {
                    case WarcraftMessageType.Say:
                        return System.Windows.Media.Brushes.White;
                    case WarcraftMessageType.Yell:
                        return System.Windows.Media.Brushes.Red;
                    case WarcraftMessageType.PrivateMessageBnet:
                        return System.Windows.Media.Brushes.LightSkyBlue;
                    case WarcraftMessageType.PrivateMessageIngame:
                        return System.Windows.Media.Brushes.Purple;
                    case WarcraftMessageType.Channel:
                        return System.Windows.Media.Brushes.DarkGray;
                    case WarcraftMessageType.Guild:
                        return System.Windows.Media.Brushes.Green;
                    case WarcraftMessageType.Instance:
                    case WarcraftMessageType.InstanceLeader:
                    case WarcraftMessageType.Raid:
                    case WarcraftMessageType.RaidLeader:
                        return System.Windows.Media.Brushes.OrangeRed;
                    case WarcraftMessageType.RaidWarning:
                        return System.Windows.Media.Brushes.DarkRed;
                    case WarcraftMessageType.Party:
                    case WarcraftMessageType.PartyLeader:
                        return System.Windows.Media.Brushes.Blue;
                    default:
                        return System.Windows.Media.Brushes.Black;
                }
            }
        }
        private SnooperExportedWarcraftMessage() : base() { } //EF
        public SnooperExportedWarcraftMessage(SnooperExportBase exportBase) : base(exportBase) { }
    }
}