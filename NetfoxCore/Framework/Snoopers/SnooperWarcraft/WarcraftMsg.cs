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

using System;
using Netfox.Snoopers.SnooperWarcraft.Models;

namespace Netfox.Snoopers.SnooperWarcraft
{

    #region WarcraftMsg
    public class WarcraftMsg
    {
        private const string PrivateMessageSpecifier = "To ";
        private string _line;
        public string MessageContent;
        public string Text;
        public string Sender;
        public string Receiver;
        public DateTime Timestamp;
        public WarcraftMessageType MessageType = WarcraftMessageType.Say;
        public bool Valid = true;

        public WarcraftMsg(string line)
        {
            // fill default values and store things we'll need later
            this._line = line;
            this.MessageContent = line;
            this.Sender = this.Receiver = this.Text = string.Empty;

            // do the parsing itself
            this.Parse();
        }

        private void Parse()
        {
            // structure follows
            // month/day h:m:s.ms type name content
            // type is determined on first character
            // name is determined based on type can be until : or not
            // if name doesnt have space and has '-' then its player chat
            // content can be either chat data or some other information

            this.Timestamp = this.GetTimestamp(this.GetStringFromLineUpToSpace(), this.GetStringFromLineUpToSpace());
            this._line = this._line.Substring(1); // skip one redundant space

            // determine chat type
            var type = this._line[0];
            switch (type)
            {
                case '[': // channel
                    this.ParseChannel();
                    break;
                case '|': // specific channel(guild,instance,battleground,whisper from someone), type determined in parsing
                    this.ParseSpecificChannel();                    
                    break;
                default: // other types (say,yell, sent whisper)
                    this.ParseOtherChat();
                    break;
            }
        }

        private DateTime GetTimestamp(string monthDay, string hourMinuteSecond)
        {
            var colonIndex = hourMinuteSecond.IndexOf(":", StringComparison.Ordinal);
            var month = Convert.ToInt32(monthDay.Substring(0, monthDay.IndexOf('/')));
            var day   = Convert.ToInt32(monthDay.Substring(monthDay.IndexOf('/') + 1));
            var hour  = Convert.ToInt32(hourMinuteSecond.Substring(0, colonIndex));
            hourMinuteSecond = hourMinuteSecond.Substring(colonIndex + 1);
            colonIndex = hourMinuteSecond.IndexOf(":", StringComparison.Ordinal);
            var min   = Convert.ToInt32(hourMinuteSecond.Substring(0, colonIndex));
            var sec   = Convert.ToInt32(hourMinuteSecond.Substring(colonIndex + 1, 2));
            // var ms = Convert.ToInt32(hourMinuteSecond.Substring(colonIndex + 3)); // if required info is there
            return new DateTime(DateTime.Now.Year, month, day, hour, min, sec);
        }

        private string GetStringFromLineUpToSpace()
        {
            // gets substring before space, and removes it from global string
            var spaceIndex = this._line.IndexOf(" ", StringComparison.Ordinal);
            if (spaceIndex == -1) return string.Empty;
            var str = this._line.Substring(0, spaceIndex);
            this._line = this._line.Substring(spaceIndex + 1);
            return str;
        }

        private void ParseChannel()
        {
            // 4/17 18:44:48.765  [1. General] Reapert: Burn xD
            // 4/26 15:39:15.956  [LocalDefense] Drulgir: bergruu up
            // 12/17 00:05:59.794  [5. world] Anuraj left channel.
            this.MessageType = WarcraftMessageType.Channel;
            var channelName = this.GetStringFromLineUpToSpace();
            if (channelName[channelName.Length - 1] != ']')
                channelName += " " + this.GetStringFromLineUpToSpace();
            channelName = channelName.Substring(1, channelName.Length - 2);

            if (channelName == "Raid Warning")
                this.MessageType = WarcraftMessageType.RaidWarning;    

            // get sender name
            var playerName = this.GetStringFromLineUpToSpace();
            if (playerName[playerName.Length - 1] != ':') // has to be chat message, not join,left spam
            {
                this.Valid = false;
                return;
            }
            playerName = playerName.Substring(0, playerName.Length - 1);

            // save text
            var text = this._line;

            // store data in object
            this.Text = text;
            this.Sender = playerName;
            this.Receiver = "Channel " + channelName;
        }

        private void ParseSpecificChannel()
        {
            // 4/18 21:42:03.279  |Kb6|k0000000|k whispers: welcome back Neth :)...I was thinking of sending you a little present, its been a while :D
            if (this._line[1] == 'K')
            {
                // skip unknown sender name and "whispers:"
                this.GetStringFromLineUpToSpace();
                this.GetStringFromLineUpToSpace();

                // set type, sender and receiver
                this.MessageType = WarcraftMessageType.PrivateMessageBnet;
                this.Sender = "Some Battle.net friend";
                this.Receiver = "Current player name";
            }
            else
            {
                var type = this._line.Substring(0, this._line.IndexOf("]|h", StringComparison.Ordinal));
                type = type.Substring(type.IndexOf("|h[", StringComparison.Ordinal)+3);
                switch (type)
                {
                    case "Guild": // 4/22 11:02:32.038  |Hchannel:GUILD|h[Guild]|h Elgreco: #bored day work
                        this.MessageType = WarcraftMessageType.Guild;
                        this.Receiver = type;
                        break;
                    case "Instance": // 4/18 14:32:25.328  |Hchannel:INSTANCE_CHAT|h[Instance]|h Nethielka-Magtheridon: a pet
                        this.MessageType = WarcraftMessageType.Instance;
                        this.Receiver = type;
                        break;
                    case "Instance Leader": // 4/18 14:24:36.882  |Hchannel:INSTANCE_CHAT|h[Instance Leader]|h Dreamxx-BronzeDragonflight: tank!!?
                        this.MessageType = WarcraftMessageType.InstanceLeader;
                        this.Receiver = "Instance";
                        this.GetStringFromLineUpToSpace();
                        break;
                    case "Raid": // 4/18 14:32:25.328  |Hchannel:RAID_CHAT|h[Raid]|h Nethielka-Magtheridon: a pet
                        this.MessageType = WarcraftMessageType.Raid;
                        this.Receiver = type;
                        break;
                    case "Raid Leader": // 4/18 14:32:25.328  |Hchannel:RAID_CHAT|h[Raid Leader]|h Nethielka-Magtheridon: a pet
                        this.MessageType = WarcraftMessageType.RaidLeader;
                        this.Receiver = "Raid";
                        this.GetStringFromLineUpToSpace();
                        break;
                    case "Party": // 4/18 17:50:50.090  |Hchannel:PARTY|h[Party]|h Nethielka-Magtheridon: you are a lifesaver
                        this.MessageType = WarcraftMessageType.Party;
                        this.Receiver = type;
                        break;
                    case "Party Leader": // 4/18 17:50:58.666  |Hchannel:PARTY|h[Party Leader]|h Quakebane-Magtheridon: :)
                        this.MessageType = WarcraftMessageType.PartyLeader;
                        this.Receiver = "Party";
                        this.GetStringFromLineUpToSpace();
                        break;
                    // not supported for example pet battle    
                    default: // 4/22 10:25:01.962  |Hchannel:PET_BATTLE_COMBAT_LOG|h[Pet Battle]|h: Battle Recovery healed 495 damage from your
                        this.Valid = false;
                        return;
                }

                // skip identification and get name
                this.GetStringFromLineUpToSpace();
                var name = this.GetStringFromLineUpToSpace();
                this.Sender = name.Substring(0, name.Length - 1);
            }

            // save text itself
            this.Text = this._line;
        }

        private void ParseOtherChat()
        {
            // sending whisper
            // 12/17 09:06:21.040  To Akiyoto-Magtheridon: just testing something, sorry for hte bother
            // 4/18 21:46:23.636  To |Kb6|k0000000|k: really? -- battle net chat, not able to get name
            if (this._line.Substring(0, 3).Equals(PrivateMessageSpecifier))
            {
                this.Sender = "Current player name";
                // skip "To "
                this.GetStringFromLineUpToSpace();
                var name = this.GetStringFromLineUpToSpace();
                if (name[0] == '|') // bnet message
                {
                    this.MessageType = WarcraftMessageType.PrivateMessageBnet;
                    this.Receiver = "Some Battle.net friend";
                }
                else // ingame message
                {
                    this.MessageType = WarcraftMessageType.PrivateMessageIngame;
                    this.Receiver = name.Substring(0, name.Length - 1);
                }
            }
            else
            {
                var name = this.GetStringFromLineUpToSpace();
                // check if it does not contain player chat
                // 12/18 11:50:30.514  You perform Herb Gathering on Frostweed.
                if (name.IndexOf('-') == -1)
                {
                    this.Valid = false;
                    return;
                }
                this.Sender = name.Substring(0, name.Length - 1);
                var type = this.GetStringFromLineUpToSpace();
                // determine type of message
                switch(type)
                {
                    case "says:":
                        this.MessageType = WarcraftMessageType.Say;
                        this.Receiver = "Nearby players";
                        break;
                    case "yells:":
                        this.MessageType = WarcraftMessageType.Yell;
                        this.Receiver = "Nearby players";
                        break;
                    case "whispers:":
                        this.MessageType = WarcraftMessageType.PrivateMessageIngame;
                        this.Receiver = "Current player name";
                        break;
                    default:
                        this.Valid = false;
                        return;
                }
            }

            // save text itself
            this.Text = this._line;
        }
    }

    #endregion WarcraftMsg
}