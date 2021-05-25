using System;
using System.Collections.Generic;
using System.Linq;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.TCP
{
    internal class TCPFlowChecker
    {
        public static List<PmFrameBase> GetSynFrames(IEnumerable<PmFrameBase> tcpFlow)
        {
            return tcpFlow.Where(frame => frame.TcpFSyn && !frame.TcpFAck).OrderBy(frame => frame.RealWindowSize)
                .Distinct(new TCPFrameSyncDistinct()).ToList();
        }

        private class TCPFrameSyncDistinct : IEqualityComparer<PmFrameBase>
        {
            public Boolean Equals(PmFrameBase x, PmFrameBase y) => x.TcpSequenceNumber == y.TcpSequenceNumber &&
                                                                   x.TcpAcknowledgementNumber ==
                                                                   y.TcpAcknowledgementNumber;

            public Int32 GetHashCode(PmFrameBase obj) =>
                obj.TcpSequenceNumber.GetHashCode() ^ obj.TcpAcknowledgementNumber.GetHashCode();
        }
    }
}