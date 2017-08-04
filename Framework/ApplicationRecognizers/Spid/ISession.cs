using System;
using System.Net;
using PacketParser;
using ProtocolIdentification;

namespace Spid
{
    internal interface ISession
    {
        ProtocolModel ApplicationProtocolModel { get; }
        IPAddress ServerIP { get; }
        IPAddress ClientIP { get; }
        UInt16 ClientPort { get; }
        UInt16 ServerPort { get; }
        DateTime FirstPacketTimestamp { get; }
        SessionHandler.TransportProtocol TransportProtocol { get; }
        DateTime LastPacketTimestamp { get; }
        String Identifier { get; }
        AttributeFingerprintHandler.PacketDirection AddFrame(Frame frame);
    }
}