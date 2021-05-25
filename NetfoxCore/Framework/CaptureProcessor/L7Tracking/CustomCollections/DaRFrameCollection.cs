using System.Collections.Generic;
using System.Linq;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections
{
    /// <summary> Collection of da r frames.</summary>
    internal class DaRFrameCollection: LinkedIterableList<PmFrameBase>
    {
        /// <summary> Constructor.</summary>
        /// <param name="collection"> The collection. </param>
        public DaRFrameCollection(IEnumerable<PmFrameBase> pmFrames)
        {
            //var pmFrames = collection as PmFrameBase[] ?? collection.ToArray();
            if (!pmFrames.Any())
            {
                return;
            }

            switch (pmFrames.First().IpProtocol)
            {
                case IPProtocolType.TCP:
                    this.InitCollection(pmFrames.OrderBy(frame => frame.TcpSequenceNumber)
                        .ThenBy(frame => frame.L7PayloadLength));
                    break;
                default:
                    this.InitCollection(pmFrames);
                    break;
            }
        }

        /// <summary> Initialises the collection.</summary>
        /// <param name="collection"> The collection. </param>
        private void InitCollection(IEnumerable<PmFrameBase> collection)
        {
            foreach (var pmFrame in collection)
            {
                this.AddLast(pmFrame);
            }
        }
    }
}