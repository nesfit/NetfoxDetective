using System;
using System.Net;
using PacketParser;
using PacketParser.Packets;
using ProtocolIdentification;

namespace Spid
{
    internal class UdpSession : ISession
    {
        public delegate void ProtocolModelCompletedEventHandler(UdpSession session, ProtocolModel protocolModel);

        //definition of an UDP session:
        //server and client IP and UDP port is the same
        //at least one packet is sent in each direction (to avoid classifying UDP port scannings as sessions)
        //the host that sends the first packet is the client
        //a session is terminated when no packet is sent in TIMEOUT (currently 60 seconds)

        public delegate void SessionEstablishedEventHandler(UdpSession session);

        private Boolean clientToServerPacketReceived;
        private Configuration config;
        private Int32 frameCount;
        private Boolean serverToClientPacketReceived;
        private Boolean usePlaceholderProtocolModel;
        public UdpSession(Configuration config, ProtocolModel applicationProtocolModel) : this(config) { this.ApplicationProtocolModel = applicationProtocolModel; }

        public UdpSession(Configuration config, IPAddress clientIp, UInt16 clientPort, IPAddress serverIp, UInt16 serverPort) : this(config)
        {
            this.ClientIP = clientIp;
            this.ClientPort = clientPort;
            this.ServerIP = serverIp;
            this.ServerPort = serverPort;
        }

        public UdpSession(Configuration config)
        {
            this.config = config;
            this.ApplicationProtocolModel = null; //protocol model is not initialized until the session is established
            this.FirstPacketTimestamp = DateTime.MinValue;
            this.ServerIP = null;
            this.ClientIP = null;
            this.usePlaceholderProtocolModel = false;
            this.serverToClientPacketReceived = false;
            this.clientToServerPacketReceived = false;
        }

        public AttributeFingerprintHandler.PacketDirection AddFrame(Frame frame)
        {
            //ProtocolIdentification.AttributeFingerprintHandler.PacketDirection packetDirection=ProtocolIdentification.AttributeFingerprintHandler.PacketDirection.Unknown;
            //let's get the IP and TCP packets from the frame
            AbstractPacket ipPacket;
            UdpPacket udpPacket;
            if(SessionHandler.TryGetIpAndUdpPackets(frame, out ipPacket, out udpPacket))
            {
                //we now have the IP and TCP packets
                var sourceIp = IPAddress.None;
                var destinationIp = IPAddress.None;
                var applicationLayerProtocolLength = 0;

                if(ipPacket.GetType() == typeof(IPv4Packet))
                {
                    var ipv4Packet = (IPv4Packet) ipPacket;
                    sourceIp = ipv4Packet.SourceIPAddress;
                    destinationIp = ipv4Packet.DestinationIPAddress;
                    applicationLayerProtocolLength = ipv4Packet.TotalLength - ipv4Packet.HeaderLength - udpPacket.DataOffsetByteCount;
                }
                else if(ipPacket.GetType() == typeof(IPv6Packet))
                {
                    var ipv6Packet = (IPv6Packet) ipPacket;
                    sourceIp = ipv6Packet.SourceIP;
                    destinationIp = ipv6Packet.DestinationIP;
                    applicationLayerProtocolLength = ipv6Packet.PayloadLength - udpPacket.DataOffsetByteCount;
                }

                //Check if the client and server have been defined
                if(this.ServerIP == null || this.ClientIP == null)
                {
                    //the first host to send a packet will be assumed to be the client
                    this.ClientIP = sourceIp;
                    this.ClientPort = udpPacket.SourcePort;
                    this.ServerIP = destinationIp;
                    this.ServerPort = udpPacket.DestinationPort;
                }

                //identify the direction
                if(sourceIp.Equals(this.ClientIP) && destinationIp.Equals(this.ServerIP) && udpPacket.SourcePort == this.ClientPort && udpPacket.DestinationPort == this.ServerPort)
                {
                    //AddFrame(ipPacket, tcpPacket, ProtocolIdentification.AttributeFingerprintHandler.PacketDirection.ClientToServer);
                    this.AddFrame(udpPacket, AttributeFingerprintHandler.PacketDirection.ClientToServer, applicationLayerProtocolLength);
                    return AttributeFingerprintHandler.PacketDirection.ClientToServer;
                }
                if(sourceIp.Equals(this.ServerIP) && destinationIp.Equals(this.ClientIP) && udpPacket.SourcePort == this.ServerPort && udpPacket.DestinationPort == this.ClientPort)
                {
                    this.AddFrame(udpPacket, AttributeFingerprintHandler.PacketDirection.ServerToClient, applicationLayerProtocolLength);
                    return AttributeFingerprintHandler.PacketDirection.ServerToClient;
                }
                throw new Exception("IP's and ports do not match those belonging to this session\nSession server: " + this.ServerIP + ":" + this.ServerPort + "\nSession client: "
                                    + this.ClientIP + ":" + this.ClientPort + "\nPacket source: " + sourceIp + ":" + udpPacket.SourcePort + "\nPacket destination: " + destinationIp
                                    + ":" + udpPacket.DestinationPort);
            }
            return AttributeFingerprintHandler.PacketDirection.Unknown;
        }

        public ProtocolModel ApplicationProtocolModel { get; private set; }
        public IPAddress ClientIP { get; private set; }
        public UInt16 ClientPort { get; private set; }
        public DateTime FirstPacketTimestamp { get; private set; }

        public String Identifier => SessionHandler.GetSessionIdentifier(this.ClientIP, this.ClientPort, this.ServerIP, this.ServerPort, this.TransportProtocol);

        public DateTime LastPacketTimestamp { get; private set; }
        public IPAddress ServerIP { get; private set; }
        public UInt16 ServerPort { get; private set; }

        public SessionHandler.TransportProtocol TransportProtocol => SessionHandler.TransportProtocol.UDP;

        public event ProtocolModelCompletedEventHandler ProtocolModelCompleted;
        public event SessionEstablishedEventHandler SessionEstablished;

        private void AddFrame(UdpPacket udpPacket, AttributeFingerprintHandler.PacketDirection direction, Int32 protocolPacketLength)
        {
            this.frameCount++;
            this.LastPacketTimestamp = udpPacket.ParentFrame.Timestamp;

            if(this.FirstPacketTimestamp == DateTime.MinValue) { this.FirstPacketTimestamp = udpPacket.ParentFrame.Timestamp; }

            //see if the session has just been established
            if(direction == AttributeFingerprintHandler.PacketDirection.ClientToServer && !this.clientToServerPacketReceived)
            {
                this.clientToServerPacketReceived = true;
                if(this.serverToClientPacketReceived && this.SessionEstablished != null) { this.SessionEstablished(this); }
            }
            else if(direction == AttributeFingerprintHandler.PacketDirection.ServerToClient && !this.serverToClientPacketReceived)
            {
                this.serverToClientPacketReceived = true;
                if(this.clientToServerPacketReceived && this.SessionEstablished != null) { this.SessionEstablished(this); }
            }

            if(this.frameCount == this.config.MaxFramesToInspectPerSession + 1 && this.ApplicationProtocolModel != SessionHandler.PlaceholderProtocolModel)
            {
                if(this.ProtocolModelCompleted != null) { this.ProtocolModelCompleted(this, this.ApplicationProtocolModel); }
                if(this.usePlaceholderProtocolModel) { this.ApplicationProtocolModel = SessionHandler.PlaceholderProtocolModel; }
            }
            if(this.frameCount <= this.config.MaxFramesToInspectPerSession)
            {
                if(protocolPacketLength > 0)
                {
                    if(this.ApplicationProtocolModel == null) { this.ApplicationProtocolModel = new ProtocolModel(this.Identifier, this.config.ActiveAttributeMeters); }
                    var protocolPacketStartIndex = udpPacket.PacketStartIndex + udpPacket.DataOffsetByteCount;
                    this.ApplicationProtocolModel.AddObservation(udpPacket.ParentFrame.Data, protocolPacketStartIndex, protocolPacketLength, udpPacket.ParentFrame.Timestamp,
                        direction);
                }
            }
        }
    }
}