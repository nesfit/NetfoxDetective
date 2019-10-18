/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
namespace Netfox.Framework.Models.PmLib.SupportedTypes
{
    /// <summary>
    ///     More general frame type Base enum definde in IFrameworkIOController so values could be
    ///     exported out of framework without referencing PmLib.
    /// </summary>
    public enum PmFrameType
    {
        /// <summary>
        ///     Frame will be parsed as LibPCAP
        /// </summary>
        Pcap,

        /// <summary>
        ///     Frame will be parsed as PCAPng
        /// </summary>
        PcapNg,

        /// <summary>
        ///     Frame will be parsed as Microsoft Network Monitor
        /// </summary>
        Mnm,

        /// <summary>
        ///     Physically non-existing frame in PCAP file.
        /// </summary>
        /// <seealso cref="Netfox.Framework.Models.PmLib.Frames.PmFrameVirtual"/>
        Virtual,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content all zeros for DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirtualBlank,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content of predefined noise DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirutalNoise,

        /// <summary>
        ///     Physically non-existing frame in PCAP file. Frame encapsulated in one or more carrier datagrams, where carrier
        ///     datagrams can be either base band frames or encapsulation packets (GSE, GRE, etc).
        /// </summary>
        Encapsulated
    }
}