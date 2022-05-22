using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Netfox.FrameworkAPI.Tests
{
    [TestFixture]
    public class PcapIntegrityTests
    {
        private static readonly int PCAP_SIGNATURE_LENGTH = 4;

        private static readonly byte[][] PCAP_SIGNATURE =
        {
            new byte[]
            {
                0x0a, 0x0d, 0x0d, 0x0a // PCAP Next Generation dump file format
            },
            new byte[]
            {
                0x47, 0x4d, 0x42, 0x55 // Microsoft Network Monitor capture file
            },
            new byte[]
            {
                0xa1, 0xb2, 0xc3, 0xd4 // Libpcap File format (LSB)
            },
            new byte[]
            {
                0xd4, 0xc3, 0xb2, 0xa1 // Libpcap File Format (MSB)
            }
        };

        [Test]
        public void IntegrityTest()
        {
            Span<byte> sig = stackalloc byte[PCAP_SIGNATURE.Length];

            foreach (var pcap in typeof(PcapPath.Pcaps).GetEnumNames().Select(Enum.Parse<PcapPath.Pcaps>))
            {
                string file = PcapPath.GetPcap(pcap);
                Assert.IsTrue(File.Exists(file),
                    $"The associated file `{file}` for test PCAP {pcap:G} does not exist!");

                using var f = File.OpenRead(file);
                Assert.AreEqual(PCAP_SIGNATURE.Length, f.Read(sig), $"File too short to be a PCAP! (path: `{file}`)");

                var any = false;
                foreach (var array in PCAP_SIGNATURE)
                {
                    if (!BytesEqual(sig, array))
                        continue;

                    any = true;
                    break;
                }
                
                Assert.IsTrue(any, $"PCAP signature doesn't match! (path: `{file}`)");
            }
        }

        private bool BytesEqual(Span<byte> left, Span<byte> right)
        {
            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
                if (left[i] != right[i])
                    return false;

            return true;
        }
    }
}