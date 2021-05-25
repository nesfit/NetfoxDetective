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
using System.Collections.Generic;
using System.Linq;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.AppIdent {
    public class AppTagProvider : IAppTagProvider
    {
        public String GetAppTagShort(IEnumerable<PmCaptureBase> captures, PmFrameBase frame)
        {
            var capture = captures.FirstOrDefault(c => c is PmCaptureMnm) as PmCaptureMnm;
            var byProtocolPortNumber = this.GetFromProtocolPortNumber(frame.SrcPort, frame.DstPort);
            var captureAppTag = String.Empty;
            if (capture != null)
                captureAppTag = capture.GetApplicationTag(frame.SourceEndPoint, frame.DestinationEndPoint);
            return this.FormatAppTag(byProtocolPortNumber, frame.IpProtocol, frame.DestinationEndPoint.Port, captureAppTag);
        }

        public String GetAppTagShort(string appTag)
        {
            return appTag.Split('-')[0];
        }

        private string FormatAppTag(string protocolPortNumber, IPProtocolType transportProtocol, int port, string application)
        {
            return protocolPortNumber != this._portProtocol[0] ? $"{transportProtocol}_{protocolPortNumber}-{application}" : $"{port}_{transportProtocol}-{application}";
        }

        private string GetFromProtocolPortNumber(int sourcePort, int destPort)
        {
            string appTag = null;
            if (this._portProtocol.TryGetValue(sourcePort, out appTag)) return appTag;
            if (this._portProtocol.TryGetValue(destPort, out appTag)) return appTag;
            return this._portProtocol[0];
        }

        private readonly Dictionary<int, string> _portProtocol = new Dictionary<int, string>()
        {
            {0,"NotKnown"},
            {20,"FtPdata"},
            {21,"FtPcomm"},
            {22,"SSH"},
            {23,"Telnet"},
            {53,"Dns"},
            {67,"DhcPc"},
            {68,"DhcPs"},
            {69,"Tftp"},
            {80,"HTTP"},
            {110,"POP3"},
            {137,"netbiosns"},
            {138,"netbiosds"},
            {139,"netbiosss"},
            {143,"IMAP"},
            {443,"Https"},//TLSSSL
            {465,"SMTPtlsSSL"},//TLSSSL
            {546,"DhcPv6C"},
            {547,"DhcPv6"},
            {995,"POP3tlsSSL"},//TLSSSL

            {1900,"SSDP"},
            {2869,"icslap"},
            {3128,"HTTP"}, //3proxy
            {3478,"STUN"},//Simple Traversal of UDP Through NAT (STUN)
            {3702,"wsd"},
            {4070,"tripe"},

            {5000,"RTP"},
            {5001,"RTP"},
            {5002,"RTP"},
            {5003,"RTP"},
            {5004,"RTP"},
            {5005,"RTP"},
            {5006,"RTP"},
            {5007,"RTP"},
            {5008,"RTP"},
            {5009,"RTP"},
            {5010,"RTP"},

            {5060,"SIP"},
            {5190,"ICQ"},
            {5222,"Jabber"},
            {5223,"JabberSsl"},
            {5351,"mdns"},
            {5355,"LLMNR"},
            {5938 ,"TeamViewer"},
            {6969 ,"HTTP"},//Backdoor.Assasin.D trojan
            {9875 ,"sapv1"},
            {12350 ,"skype"},

            {27000,"OnlineGames"},
            {27001,"OnlineGames"},
            {27002,"OnlineGames"},
            {27003,"OnlineGames"},
            {27004,"OnlineGames"},
            {27005,"OnlineGames"},
            {27006,"OnlineGames"},
            {27007,"OnlineGames"},
            {27008,"OnlineGames"},
            {27009,"OnlineGames"},
            {27010,"OnlineGames"},
            {27011,"OnlineGames"},
            {27012,"OnlineGames"},
            {27013,"OnlineGames"},
            {27014,"OnlineGames"},
            {27015,"OnlineGames"},
            {27016,"OnlineGames"},
            {27017,"OnlineGames"},
            {27018,"OnlineGames"},
            {27019,"OnlineGames"},
            {27020,"OnlineGames"},
            {27021,"OnlineGames"},
            {27022,"OnlineGames"},
            {27023,"OnlineGames"},
            {27024,"OnlineGames"},
            {27025,"OnlineGames"},
            {27026,"OnlineGames"},
            {27027,"OnlineGames"},
            {27028,"OnlineGames"},
            {27029,"OnlineGames"},
            {27030,"OnlineGames"},
            {27031,"OnlineGames"},
            {27032,"OnlineGames"},
            {27033,"OnlineGames"},
            {27034,"OnlineGames"},
            {27035,"OnlineGames"},
            {27036,"OnlineGames"},
            {27037,"OnlineGames"},
            {27038,"OnlineGames"},
            {27039,"OnlineGames"},
            {27040,"OnlineGames"},
            {27041,"OnlineGames"},
            {27042,"OnlineGames"},
            {27043,"OnlineGames"},
            {27044,"OnlineGames"},
            {27045,"OnlineGames"},
            {27046,"OnlineGames"},
            {27047,"OnlineGames"},
            {27048,"OnlineGames"},
            {27049,"OnlineGames"},
            {27050,"OnlineGames"},

            {6881,"P2PBittorrent"},
            {6882,"P2PBittorrent"},
            {6883,"P2PBittorrent"},
            {6884,"P2PBittorrent"},
            {6885,"P2PBittorrent"},
            {6886,"P2PBittorrent"},
            {6887,"P2PBittorrent"},
            {6888,"P2PBittorrent"},
            {6889,"P2PBittorrent"},
            {6890,"P2PBittorrent"},
            {6891,"P2PBittorrent"},
            {6892,"P2PBittorrent"},
            {6893,"P2PBittorrent"},
            {6894,"P2PBittorrent"},
            {6895,"P2PBittorrent"},
            {6896,"P2PBittorrent"},
            {6897,"P2PBittorrent"},
            {6898,"P2PBittorrent"},
            {6899,"P2PBittorrent"},
            {6900,"P2PBittorrent"},
            {51413,"P2PBittorrent"},

            {57621,"spotify"}
        };
    }
}