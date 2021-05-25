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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Netfox.NBARDatabase
{
    /// <summary>
    ///     TODO implement auto protocol insertion from Sleuth assemblies InbarProtocolPortDatabase servers
    ///     as basic mapper of L4 port to L7 protocol name.
    /// </summary>
    public class NBARProtocolPortDatabase
    {
        private static readonly ConcurrentDictionary<String, NBAR2Taxonomy> TaxonomyFilePathToTaxanomyDictionary =
            new ConcurrentDictionary<String, NBAR2Taxonomy>();

        private readonly ConcurrentDictionary<String, NBAR2TaxonomyProtocol> ProtocolTaxonomyDictionary;
        private readonly ConcurrentDictionary<UInt32, List<NBAR2TaxonomyProtocol>> ProtocolTCPPortDictionary;
        private readonly ConcurrentDictionary<UInt32, List<NBAR2TaxonomyProtocol>> ProtocolUDPPortDictionary;

        private Object _taxonomyLoaderLock = new Object();

        public NBARProtocolPortDatabase()
        {
            Assembly self = typeof(NBARProtocolPortDatabase).Assembly;
            string dir = Path.GetDirectoryName(self.Location);
            string taxonomyFilePath = Path.Join(dir, NBARFiles.TAXONOMY_FILE);
            if (!File.Exists(taxonomyFilePath))
                throw new ArgumentException($"Database file for NBAR protocol ports not found in `{taxonomyFilePath}`!");

            this.ProtocolTCPPortDictionary = new ConcurrentDictionary<UInt32, List<NBAR2TaxonomyProtocol>>();
            this.ProtocolUDPPortDictionary = new ConcurrentDictionary<UInt32, List<NBAR2TaxonomyProtocol>>();

            this.ProtocolTaxonomyDictionary =
                new ConcurrentDictionary<String, NBAR2TaxonomyProtocol>(StringComparer.CurrentCultureIgnoreCase);

            lock (this._taxonomyLoaderLock)
            {
                if (TaxonomyFilePathToTaxanomyDictionary.ContainsKey(taxonomyFilePath))
                {
                    this.Nbar2Taxonomy = TaxonomyFilePathToTaxanomyDictionary[taxonomyFilePath];
                }
                else
                {
                    this.Nbar2Taxonomy = NBAR2Taxonomy.Nbar2TaxonomyLoader(taxonomyFilePath);
                    TaxonomyFilePathToTaxanomyDictionary.AddOrUpdate(taxonomyFilePath, this.Nbar2Taxonomy,
                        (s, taxonomy) => this.Nbar2Taxonomy = taxonomy);
                }
            }

            foreach (var protocol in this.Nbar2Taxonomy.NBAR2TaxonomyProtocol) //TODO for parallel
            {
                foreach (var port in protocol.ports)
                {
                    foreach (var tcpPort in port.tcpArr)
                    {
                        this.ProtocolTCPPortDictionary.AddOrUpdate(tcpPort, u => new List<NBAR2TaxonomyProtocol>
                        {
                            protocol
                        }, (u, list) =>
                        {
                            list.Add(protocol);
                            return list;
                        });
                    }

                    foreach (var udpPort in port.udpArr)
                    {
                        this.ProtocolUDPPortDictionary.AddOrUpdate(udpPort, u => new List<NBAR2TaxonomyProtocol>
                        {
                            protocol
                        }, (u, list) =>
                        {
                            list.Add(protocol);
                            return list;
                        });
                    }
                }

                this.ProtocolTaxonomyDictionary.AddOrUpdate(protocol.name, u => protocol,
                    (u, proto) =>
                        throw new ArgumentException("Key already exists, check NBAR taxonomy database file",
                            "protocol"));
            }

            this.TCPProtocolsAndPorts = new Dictionary<UInt32, Tuple<String[], List<NBAR2TaxonomyProtocol>>>();
            foreach (var portProtocol in this.ProtocolTCPPortDictionary)
            {
                var port = portProtocol.Key;
                var taxList = portProtocol.Value.Select(tax => tax.name);
                var protocolList = portProtocol.Value;
                protocolList.Sort(ProtocolTaxonomyPortComparison);
                var tuple = Tuple.Create(taxList.ToArray(), protocolList);
                this.TCPProtocolsAndPorts.Add(port, tuple);
            }

            this.UDPProtocolsAndPorts = new Dictionary<UInt32, Tuple<String[], List<NBAR2TaxonomyProtocol>>>();
            foreach (var portProtocol in this.ProtocolUDPPortDictionary)
            {
                var port = portProtocol.Key;
                var taxList = portProtocol.Value.Select(tax => tax.name);
                var protocolList = portProtocol.Value;
                protocolList.Sort(ProtocolTaxonomyPortComparison);
                var tuple = Tuple.Create(taxList.ToArray(), protocolList);
                this.UDPProtocolsAndPorts.Add(port, tuple);
            }
        }

        public Dictionary<UInt32, Tuple<String[], List<NBAR2TaxonomyProtocol>>> TCPProtocolsAndPorts { get; }
        public Dictionary<UInt32, Tuple<String[], List<NBAR2TaxonomyProtocol>>> UDPProtocolsAndPorts { get; }
        public NBAR2Taxonomy Nbar2Taxonomy { get; private set; }

        /// <summary>
        ///     Gets an Array of NBAR2TaxonomyProtocol containing given protocol name
        /// </summary>
        /// <returns>Array of NBAR2TaxonomyProtocol containing given protocol name</returns>
        public NBAR2TaxonomyProtocol GetNbar2TaxonomyProtocol(String nbarProtocolName)
        {
            NBAR2TaxonomyProtocol taxonomy;
            if (nbarProtocolName == null)
            {
                return null;
            }

            this.ProtocolTaxonomyDictionary.TryGetValue(nbarProtocolName, out taxonomy);
            return taxonomy;
        }

        /// <summary> Adds ports extracted from sleuths.</summary>
        /// <summary> Determine if given port is known server side port.</summary>
        /// <param name="port"> . </param>
        /// <returns> TRUE if port is recognized as known server side port, FALSE otherwise.</returns>
        public Boolean IsTCPServerPort(Int32 port) => this.ProtocolTCPPortDictionary.ContainsKey((UInt16) port);

        /// <summary> Determine if given port is known server side port.</summary>
        /// <param name="port"> . </param>
        /// <returns> TRUE if port is recognized as known server side port, FALSE otherwise.</returns>
        public Boolean IsUDPServerPort(Int32 port) => this.ProtocolUDPPortDictionary.ContainsKey((UInt16) port);

        public static Int32 ProtocolTaxonomyPortComparison(NBAR2TaxonomyProtocol p1, NBAR2TaxonomyProtocol p2)
        {
            {
                Int32 port1;
                Int32 port2;
                Int32.TryParse(p1.selectorid, out port1);
                Int32.TryParse(p2.selectorid, out port2);
                if (port1 <= port2)
                {
                    return 1;
                }

                return -1;
            }
        }
    }
}