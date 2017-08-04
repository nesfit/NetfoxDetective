using System;
using System.Net;
using PacketParser;
using PacketParser.Packets;
using ProtocolIdentification;

namespace Spid
{
    /// <summary>
    ///     The definition of a session is:
    ///     * SYN and SYN+ACK is received
    ///     * FIN+ACK not received
    ///     * RST not received
    /// </summary>
    internal class TcpSession : ISession
    {
        public delegate void ProtocolModelCompletedEventHandler(TcpSession session, ProtocolModel protocolModel);

        public delegate void SessionClosedEventHandler(TcpSession session, ProtocolModel protocolModel);

        public delegate void SessionEstablishedEventHandler(TcpSession session);

        public enum TCPState
        {
            NONE,
            SYN,
            SYN_ACK,
            ESTABLISHED,
            FIN,
            CLOSED
        }

        //each protocol model consumes 30*256*4 = 30720 bytes (30kB)
        //private System.Net.NetworkInformation.PhysicalAddress serverMac, clientMac;
        private Configuration config;
        public TcpSession(Configuration config, ProtocolModel applicationProtocolModel) : this(config) { this.ApplicationProtocolModel = applicationProtocolModel; }

        public TcpSession(Configuration config, IPAddress clientIp, UInt16 clientPort, IPAddress serverIp, UInt16 serverPort) : this(config)
        {
            this.ClientIP = clientIp;
            this.ClientPort = clientPort;
            this.ServerIP = serverIp;
            this.ServerPort = serverPort;
        }

        public TcpSession(Configuration config)
        {
            this.config = config;
            this.ApplicationProtocolModel = null; //protocol model is not initialized until the session is established
            this.State = TCPState.NONE;
            this.FirstPacketTimestamp = DateTime.MinValue;
            this.ServerIP = null;
            this.ClientIP = null;
            this.UsePlaceholderProtocolModel = false;
        }

        public Int32 FrameCount { get; private set; }
        public TCPState State { get; set; }

        /// <summary>
        ///     The session's application protocol model can be replaced with a
        ///     "place holder" protocol model when the session or model is complete
        ///     in order to save memory.
        ///     Default value is: false
        /// </summary>
        public Boolean UsePlaceholderProtocolModel { get; set; }

        public AttributeFingerprintHandler.PacketDirection AddFrame(Frame frame)
        {
            //ProtocolIdentification.AttributeFingerprintHandler.PacketDirection packetDirection=ProtocolIdentification.AttributeFingerprintHandler.PacketDirection.Unknown;
            //let's get the IP and TCP packets from the frame
            AbstractPacket ipPacket;
            TcpPacket tcpPacket;
            if(SessionHandler.TryGetIpAndTcpPackets(frame, out ipPacket, out tcpPacket))
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
                    applicationLayerProtocolLength = ipv4Packet.TotalLength - ipv4Packet.HeaderLength - tcpPacket.DataOffsetByteCount;
                }
                else if(ipPacket.GetType() == typeof(IPv6Packet))
                {
                    var ipv6Packet = (IPv6Packet) ipPacket;
                    sourceIp = ipv6Packet.SourceIP;
                    destinationIp = ipv6Packet.DestinationIP;
                    applicationLayerProtocolLength = ipv6Packet.PayloadLength - tcpPacket.DataOffsetByteCount;
                }

                //Check if the client and server have been defined
                if(this.ServerIP == null || this.ClientIP == null)
                {
                    //find out which host is server and which is client
                    if(tcpPacket.FlagBits.Synchronize && !tcpPacket.FlagBits.Acknowledgement)
                    {
                        //source is client
                        this.ClientIP = sourceIp;
                        this.ClientPort = tcpPacket.SourcePort;
                        this.ServerIP = destinationIp;
                        this.ServerPort = tcpPacket.DestinationPort;
                    }
                    else if(tcpPacket.FlagBits.Synchronize && tcpPacket.FlagBits.Acknowledgement)
                    {
                        //source is server
                        this.ServerIP = sourceIp;
                        this.ServerPort = tcpPacket.SourcePort;
                        this.ClientIP = destinationIp;
                        this.ClientPort = tcpPacket.DestinationPort;
                    }
                    else
                    {
                        throw new Exception("Session does not start with a SYN or SYN+ACK packet");
                    }
                }

                //identify the direction
                if(sourceIp.Equals(this.ClientIP) && destinationIp.Equals(this.ServerIP) && tcpPacket.SourcePort == this.ClientPort && tcpPacket.DestinationPort == this.ServerPort)
                {
                    //AddFrame(ipPacket, tcpPacket, ProtocolIdentification.AttributeFingerprintHandler.PacketDirection.ClientToServer);
                    this.AddFrame(tcpPacket, AttributeFingerprintHandler.PacketDirection.ClientToServer, applicationLayerProtocolLength);
                    return AttributeFingerprintHandler.PacketDirection.ClientToServer;
                }
                if(sourceIp.Equals(this.ServerIP) && destinationIp.Equals(this.ClientIP) && tcpPacket.SourcePort == this.ServerPort && tcpPacket.DestinationPort == this.ClientPort)
                {
                    this.AddFrame(tcpPacket, AttributeFingerprintHandler.PacketDirection.ServerToClient, applicationLayerProtocolLength);
                    return AttributeFingerprintHandler.PacketDirection.ServerToClient;
                }
                throw new Exception("IP's and ports do not match those belonging to this session\nSession server: " + this.ServerIP + ":" + this.ServerPort + "\nSession client: "
                                    + this.ClientIP + ":" + this.ClientPort + "\nPacket source: " + sourceIp + ":" + tcpPacket.SourcePort + "\nPacket destination: " + destinationIp
                                    + ":" + tcpPacket.DestinationPort);
            }
            return AttributeFingerprintHandler.PacketDirection.Unknown;
        }

        public ProtocolModel ApplicationProtocolModel { get; private set; }
        public IPAddress ClientIP { get; set; }
        public UInt16 ClientPort { get; set; }
        public DateTime FirstPacketTimestamp { get; private set; }

        public String Identifier => SessionHandler.GetSessionIdentifier(this.ClientIP, this.ClientPort, this.ServerIP, this.ServerPort, this.TransportProtocol);

        public DateTime LastPacketTimestamp { get; private set; }
        public IPAddress ServerIP { get; set; }
        public UInt16 ServerPort { get; set; }
        //public const int PROTOCOL_MODEL_MAX_FRAMES=100;

        public SessionHandler.TransportProtocol TransportProtocol => SessionHandler.TransportProtocol.TCP;

        public void AddFrame(TcpPacket tcpPacket, AttributeFingerprintHandler.PacketDirection direction, Int32 protocolPacketLength)
        {
            this.FrameCount++;
            this.LastPacketTimestamp = tcpPacket.ParentFrame.Timestamp;

            if(this.FirstPacketTimestamp == DateTime.MinValue) { this.FirstPacketTimestamp = tcpPacket.ParentFrame.Timestamp; }

            if(direction == AttributeFingerprintHandler.PacketDirection.Unknown) { throw new Exception("A valid direction must be supplied"); }

            if(this.FrameCount == this.config.MaxFramesToInspectPerSession + 1 && this.ApplicationProtocolModel != SessionHandler.PlaceholderProtocolModel)
            {
                if(this.ProtocolModelCompleted != null) { this.ProtocolModelCompleted(this, this.ApplicationProtocolModel); }
                if(this.UsePlaceholderProtocolModel) { this.ApplicationProtocolModel = SessionHandler.PlaceholderProtocolModel; }
            }

            if(this.State == TCPState.CLOSED)
            {
                //do nothing
                //throw new Exception("Cannot add a frame to a closed session");
            }
            else if(this.State == TCPState.NONE)
            {
                //Expected: client->server SYN
                //or unidirectional server->client SYN+ACK
                if(direction == AttributeFingerprintHandler.PacketDirection.ClientToServer && tcpPacket.FlagBits.Synchronize) {
                    this.State = TCPState.SYN;
                }
                else if(direction == AttributeFingerprintHandler.PacketDirection.ServerToClient && tcpPacket.FlagBits.Synchronize && tcpPacket.FlagBits.Acknowledgement) {
                    this.State = TCPState.SYN_ACK;
                }
            }
            else if(this.State == TCPState.SYN)
            {
                //Expected: server->client SYN+ACK
                //or unidirectional client->server ACK
                if(direction == AttributeFingerprintHandler.PacketDirection.ServerToClient && tcpPacket.FlagBits.Synchronize && tcpPacket.FlagBits.Acknowledgement) {
                    this.State = TCPState.SYN_ACK;
                }
                else if(direction == AttributeFingerprintHandler.PacketDirection.ClientToServer && !tcpPacket.FlagBits.Synchronize && tcpPacket.FlagBits.Acknowledgement) {
                    this.State = TCPState.ESTABLISHED;
                }
            }
            else if(this.State == TCPState.SYN_ACK)
            {
                //Expected: client->server ACK
                //or unidirectional first data packet server->client
                if(direction == AttributeFingerprintHandler.PacketDirection.ClientToServer && tcpPacket.FlagBits.Acknowledgement && !tcpPacket.FlagBits.Synchronize)
                {
                    this.State = TCPState.ESTABLISHED;
                    //generate a SessionEstablishedEvent
                    if(this.SessionEstablished != null) { this.SessionEstablished(this); }
                }
                //else if(direction==ProtocolIdentification.AttributeFingerprintHandler.PacketDirection.ServerToClient && (ipPacket.TotalLength-(tcpPacket.PacketStartIndex-ipPacket.PacketStartIndex)-tcpPacket.DataOffsetByteCount)>0) {
                else if(direction == AttributeFingerprintHandler.PacketDirection.ServerToClient && protocolPacketLength > 0)
                {
                    this.State = TCPState.ESTABLISHED;
                    if(this.SessionEstablished != null) { this.SessionEstablished(this); }
                    //AddFrameToOpenSession(ipPacket, tcpPacket, direction);
                    this.AddFrameToOpenSession(tcpPacket, direction, protocolPacketLength);
                }
            }
            else if(this.State == TCPState.ESTABLISHED || this.State == TCPState.FIN)
            {
                //AddFrameToOpenSession(ipPacket, tcpPacket, direction);
                this.AddFrameToOpenSession(tcpPacket, direction, protocolPacketLength);
            }
        }

        public event ProtocolModelCompletedEventHandler ProtocolModelCompleted;
        public event SessionClosedEventHandler SessionClosed;
        public event SessionEstablishedEventHandler SessionEstablished;

        private void AddFrameToOpenSession(TcpPacket tcpPacket, AttributeFingerprintHandler.PacketDirection direction, Int32 protocolPacketLength)
        {
            if(this.State != TCPState.ESTABLISHED && this.State != TCPState.FIN) { throw new Exception("Session is not open!"); }
            if(this.State == TCPState.ESTABLISHED && tcpPacket.FlagBits.Fin && !tcpPacket.FlagBits.Acknowledgement) {
                this.State = TCPState.FIN;
            }
            else if(tcpPacket.FlagBits.Fin && tcpPacket.FlagBits.Acknowledgement)
            {
                this.State = TCPState.CLOSED;
                if(this.SessionClosed != null) { this.SessionClosed(this, this.ApplicationProtocolModel); }
                if(this.UsePlaceholderProtocolModel) {
                    this.ApplicationProtocolModel = SessionHandler.PlaceholderProtocolModel; //to save memory
                }
            }
            else if(tcpPacket.FlagBits.Reset)
            {
                this.State = TCPState.CLOSED;
                if(this.SessionClosed != null) { this.SessionClosed(this, this.ApplicationProtocolModel); }
                if(this.UsePlaceholderProtocolModel) {
                    this.ApplicationProtocolModel = SessionHandler.PlaceholderProtocolModel; //to save memory
                }
            }
            else
            {
                if(this.ApplicationProtocolModel == null) { this.ApplicationProtocolModel = new ProtocolModel(this.Identifier, this.config.ActiveAttributeMeters); }

                if(this.FrameCount <= this.config.MaxFramesToInspectPerSession)
                {
                    //at last we can add the observation to the model
                    //int protocolPacketStartIndex=tcpPacket.PacketStartIndex+tcpPacket.DataOffsetByteCount;
                    //int protocolPacketLength=ipPacket.TotalLength-(tcpPacket.PacketStartIndex-ipPacket.PacketStartIndex)-tcpPacket.DataOffsetByteCount;
                    if(protocolPacketLength > 0)
                    {
                        var protocolPacketStartIndex = tcpPacket.PacketStartIndex + tcpPacket.DataOffsetByteCount;
                        this.ApplicationProtocolModel.AddObservation(tcpPacket.ParentFrame.Data, protocolPacketStartIndex, protocolPacketLength, tcpPacket.ParentFrame.Timestamp,
                            direction);
                    }
                }
            }
        }
    }
}