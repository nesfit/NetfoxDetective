using System;
using System.Collections.Generic;
using System.Net;
using PacketParser;
using PacketParser.Packets;
using ProtocolIdentification;

namespace Spid
{
    internal class SessionHandler
    {
        public delegate void SessionProtocolModelCompletedEventHandler(ISession session, ProtocolModel protocolModel);

        public enum TransportProtocol
        {
            TCP,
            UDP
        }

        //60 seconds
        private static readonly TimeSpan SESSION_TIMEOUT = new TimeSpan(0, 1, 0); //60 seconds. Rossi et al uses 200s in their Skype paper
        //this protocol model is here just to save memory
        //sessions protocol models are replaced this one when
        //they have reached 100 frames
        public static ProtocolModel PlaceholderProtocolModel = new ProtocolModel("Placeholder Protocol Model", Configuration.GetInstance().ActiveAttributeMeters);
        private Configuration config;
        //the purpose with having a separate list for not-yet established
        //sessions is to prevent SYN scans from pushing established sessions
        //out from the list
        private PopularityList<String, ISession> establishedSessionsList, upcomingSessionsList;
        //10.000 simultaneous sessions might a good (high) value
        //each session will consume ~30kB
        public SessionHandler(Int32 maxSimultaneousSessions, Configuration config)
        {
            this.upcomingSessionsList = new PopularityList<String, ISession>(maxSimultaneousSessions);
            this.establishedSessionsList = new PopularityList<String, ISession>(maxSimultaneousSessions);
            this.establishedSessionsList.PopularityLost += this.establishedSessionsList_PopularityLost;
            this.config = config;
        }

        public Int32 SessionsCount => this.establishedSessionsList.Count;

        public static String GetSessionIdentifier(IPAddress clientIp, UInt16 clientPort, IPAddress serverIp, UInt16 serverPort, TransportProtocol transportProtocol)
            => transportProtocol + " " + clientIp + ":" + clientPort + " -> " + serverIp + ":" + serverPort;

        /// <summary>
        ///     To get the remaining sessions whose protocol models have not yet been fully completed
        /// </summary>
        /// <returns>Sessions</returns>
        public IEnumerable<ISession> GetSessionsWithoutCompletedProtocolModels()
        {
            foreach(var s in this.establishedSessionsList.GetValueEnumerator()) {
                if(s.ApplicationProtocolModel != null && s.ApplicationProtocolModel != PlaceholderProtocolModel) { yield return s; }
            }
        }

        public event SessionProtocolModelCompletedEventHandler SessionProtocolModelCompleted;

        public static Boolean TryGetEthernetPacket(Frame frame, out Ethernet2Packet ethernetPacket)
        {
            ethernetPacket = null;
            foreach(var p in frame.PacketList)
            {
                if(p.GetType() == typeof(Ethernet2Packet))
                {
                    ethernetPacket = (Ethernet2Packet) p;
                    return true;
                }
                if(p.GetType() == typeof(RawPacket)) { return false; }
                if(p.GetType() == typeof(IPv4Packet)) { return false; }
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="ipPacket">IPv4 or IPv6 packet in frame</param>
        /// <param name="tcpPacket">TCP packet in frame</param>
        /// <returns></returns>
        public static Boolean TryGetIpAndTcpPackets(Frame frame, out AbstractPacket ipPacket, out TcpPacket tcpPacket)
        {
            ipPacket = null;
            //sourceIp=System.Net.IPAddress.None;
            //destinationIp=System.Net.IPAddress.None;
            tcpPacket = null;
            foreach(var p in frame.PacketList)
            {
                if(p.GetType() == typeof(RawPacket)) { return false; }
                if(p.GetType() == typeof(UdpPacket)) { return false; }
                if(p.GetType() == typeof(IPv4Packet)) {
                    ipPacket = p;
                }
                else if(p.GetType() == typeof(IPv6Packet)) {
                    ipPacket = p;
                }
                else if(p.GetType() == typeof(TcpPacket))
                {
                    tcpPacket = (TcpPacket) p;
                    //there is no point in enumarating further than the TCP packet
                    if(ipPacket != null) { return true; }
                    return false;
                }
            }
            return false;
        }

        public static Boolean TryGetIpAndUdpPackets(Frame frame, out AbstractPacket ipPacket, out UdpPacket udpPacket)
        {
            ipPacket = null;
            udpPacket = null;
            foreach(var p in frame.PacketList)
            {
                if(p.GetType() == typeof(RawPacket)) { return false; }
                if(p.GetType() == typeof(TcpPacket)) { return false; }
                if(p.GetType() == typeof(IPv4Packet)) {
                    ipPacket = p;
                }
                else if(p.GetType() == typeof(IPv6Packet)) {
                    ipPacket = p;
                }
                else if(p.GetType() == typeof(UdpPacket))
                {
                    udpPacket = (UdpPacket) p;
                    //there is no point in enumarating further than the UDP packet
                    if(ipPacket != null) { return true; }
                    return false;
                }
            }
            return false;
        }

        public Boolean TryGetSession(Frame frame, out ISession session)
        {
            //start with getting the IP adresses and port numbers
            AbstractPacket ipPacket;
            TcpPacket tcpPacket;
            UdpPacket udpPacket;
            session = null;

            var sourceIp = IPAddress.None;
            var destinationIp = IPAddress.None;

            if(TryGetIpAndTcpPackets(frame, out ipPacket, out tcpPacket))
            {
                if(ipPacket.GetType() == typeof(IPv4Packet))
                {
                    var ipv4Packet = (IPv4Packet) ipPacket;
                    sourceIp = ipv4Packet.SourceIPAddress;
                    destinationIp = ipv4Packet.DestinationIPAddress;
                }
                else if(ipPacket.GetType() == typeof(IPv6Packet))
                {
                    var ipv6Packet = (IPv6Packet) ipPacket;
                    sourceIp = ipv6Packet.SourceIP;
                    destinationIp = ipv6Packet.DestinationIP;
                }
                //we now have the IP addresses
                TcpSession tcpSession;
                if(this.TryGetSession(sourceIp, destinationIp, tcpPacket, out tcpSession))
                {
                    session = tcpSession;
                    return true;
                }
                return false;
            }
            if(TryGetIpAndUdpPackets(frame, out ipPacket, out udpPacket))
            {
                if(ipPacket.GetType() == typeof(IPv4Packet))
                {
                    var ipv4Packet = (IPv4Packet) ipPacket;
                    sourceIp = ipv4Packet.SourceIPAddress;
                    destinationIp = ipv4Packet.DestinationIPAddress;
                }
                else if(ipPacket.GetType() == typeof(IPv6Packet))
                {
                    var ipv6Packet = (IPv6Packet) ipPacket;
                    sourceIp = ipv6Packet.SourceIP;
                    destinationIp = ipv6Packet.DestinationIP;
                }
                //we now have the IP addresses
                UdpSession udpSession;
                if(this.TryGetSession(sourceIp, destinationIp, udpPacket, out udpSession))
                {
                    session = udpSession;
                    return true;
                }
                return false;
            }
            return false; //no session found
        }

        public Boolean TryGetSession(IPAddress sourceIp, IPAddress destinationIp, UdpPacket udpPacket, out UdpSession session)
        {
            session = null;
            var clientToServerIdentifier = GetSessionIdentifier(sourceIp, udpPacket.SourcePort, destinationIp, udpPacket.DestinationPort, TransportProtocol.UDP);
            var serverToClientIdentifier = GetSessionIdentifier(destinationIp, udpPacket.DestinationPort, sourceIp, udpPacket.SourcePort, TransportProtocol.UDP);
            if(this.upcomingSessionsList.ContainsKey(clientToServerIdentifier)) {
                session = (UdpSession) this.upcomingSessionsList[clientToServerIdentifier];
            }
            else if(this.establishedSessionsList.ContainsKey(clientToServerIdentifier)) {
                session = (UdpSession) this.establishedSessionsList[clientToServerIdentifier];
            }
            else if(this.upcomingSessionsList.ContainsKey(serverToClientIdentifier)) {
                session = (UdpSession) this.upcomingSessionsList[serverToClientIdentifier];
            }
            else if(this.establishedSessionsList.ContainsKey(serverToClientIdentifier)) { session = (UdpSession) this.establishedSessionsList[serverToClientIdentifier]; }

            //see if the session has timed out
            if(session != null && session.LastPacketTimestamp.Add(SESSION_TIMEOUT) < udpPacket.ParentFrame.Timestamp)
            {
                //session has timed out
                if(this.establishedSessionsList.ContainsKey(session.Identifier))
                {
                    this.establishedSessionsList.Remove(session.Identifier);
                    if(this.SessionProtocolModelCompleted != null) { this.SessionProtocolModelCompleted(session, session.ApplicationProtocolModel); }
                }

                session = null;
            }

            if(session == null)
            {
                //create a new session, source is client (the UDP client is defined as the host that send the first packet)
                session = new UdpSession(this.config, sourceIp, udpPacket.SourcePort, destinationIp, udpPacket.DestinationPort);
                this.upcomingSessionsList.Add(session.Identifier, session);
                session.SessionEstablished += this.session_SessionEstablished;
                session.ProtocolModelCompleted += this.session_ProtocolModelCompleted;
            }
            if(session != null) { return true; }
            return false;
        }

        public Boolean TryGetSession(IPAddress sourceIp, IPAddress destinationIp, TcpPacket tcpPacket, out TcpSession session)
        {
            session = null;

            var clientToServerIdentifier = GetSessionIdentifier(sourceIp, tcpPacket.SourcePort, destinationIp, tcpPacket.DestinationPort, TransportProtocol.TCP);
            var serverToClientIdentifier = GetSessionIdentifier(destinationIp, tcpPacket.DestinationPort, sourceIp, tcpPacket.SourcePort, TransportProtocol.TCP);
            if(this.upcomingSessionsList.ContainsKey(clientToServerIdentifier)) {
                session = (TcpSession) this.upcomingSessionsList[clientToServerIdentifier];
            }
            else if(this.establishedSessionsList.ContainsKey(clientToServerIdentifier)) {
                session = (TcpSession) this.establishedSessionsList[clientToServerIdentifier];
            }
            else if(this.upcomingSessionsList.ContainsKey(serverToClientIdentifier)) {
                session = (TcpSession) this.upcomingSessionsList[serverToClientIdentifier];
            }
            else if(this.establishedSessionsList.ContainsKey(serverToClientIdentifier)) { session = (TcpSession) this.establishedSessionsList[serverToClientIdentifier]; }

            //see if the session has timed out
            if(session != null && session.LastPacketTimestamp.Add(SESSION_TIMEOUT) < tcpPacket.ParentFrame.Timestamp)
            {
                //session has timed out
                if(this.upcomingSessionsList.ContainsKey(session.Identifier)) {
                    this.upcomingSessionsList.Remove(session.Identifier);
                }
                else if(this.establishedSessionsList.ContainsKey(session.Identifier))
                {
                    this.establishedSessionsList.Remove(session.Identifier);
                    if(this.SessionProtocolModelCompleted != null) { this.SessionProtocolModelCompleted(session, session.ApplicationProtocolModel); }
                }

                session = null;
            }

            if(session == null)
            {
                //try to create a new session
                if(tcpPacket.FlagBits.Synchronize && !tcpPacket.FlagBits.Acknowledgement)
                {
                    //the first SYN packet - source is client
                    session = new TcpSession(this.config, sourceIp, tcpPacket.SourcePort, destinationIp, tcpPacket.DestinationPort);
                }
                else if(tcpPacket.FlagBits.Synchronize && tcpPacket.FlagBits.Acknowledgement)
                {
                    //we've missed the first SYN but caught the SYN+ACK - destination is client
                    session = new TcpSession(this.config, destinationIp, tcpPacket.DestinationPort, sourceIp, tcpPacket.SourcePort);
                    session.State = TcpSession.TCPState.SYN; //I will pretend that the SYN has already been observed
                }

                if(session != null)
                {
                    this.upcomingSessionsList.Add(session.Identifier, session);
                    session.UsePlaceholderProtocolModel = true; //in order to save memory, but I will need to detect when the protocol model is completed instead
                    session.SessionEstablished += this.session_SessionEstablished;
                    session.ProtocolModelCompleted += this.session_ProtocolModelCompleted;
                    session.SessionClosed += this.session_SessionClosed;
                }
            }
            if(session != null) { return true; }
            return false;
        }

        private void establishedSessionsList_PopularityLost(String sessionIdentifierKey, ISession session)
        {
            //now, do something smart with the session
            if(this.SessionProtocolModelCompleted != null) { this.SessionProtocolModelCompleted(session, session.ApplicationProtocolModel); }
        }

        private void session_ProtocolModelCompleted(ISession session, ProtocolModel protocolModel)
        {
            if(protocolModel != null && protocolModel != PlaceholderProtocolModel && this.SessionProtocolModelCompleted != null) {
                this.SessionProtocolModelCompleted(session, protocolModel);
            }
        }

        private void session_SessionClosed(TcpSession session, ProtocolModel protocolModel)
        {
            //first remove the session from the list
            if(this.upcomingSessionsList.ContainsKey(session.Identifier)) {
                this.upcomingSessionsList.Remove(session.Identifier);
            }
            else if(this.establishedSessionsList.ContainsKey(session.Identifier)) { this.establishedSessionsList.Remove(session.Identifier); }

            if(protocolModel != null && protocolModel != PlaceholderProtocolModel && this.SessionProtocolModelCompleted != null) {
                this.SessionProtocolModelCompleted(session, protocolModel);
            }
        }

        private void session_SessionEstablished(ISession session)
        {
            //see if the session is in the non-established list
            if(this.upcomingSessionsList.ContainsKey(session.Identifier)) { this.upcomingSessionsList.Remove(session.Identifier); }
            //now move it to the established list instead
            if(!this.establishedSessionsList.ContainsKey(session.Identifier)) { this.establishedSessionsList.Add(session.Identifier, session); }
        }
    }
}