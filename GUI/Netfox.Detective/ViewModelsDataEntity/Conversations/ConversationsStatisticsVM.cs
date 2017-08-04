// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Castle.Windsor;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.Conversations;
using Netfox.Framework.Models.Interfaces;
using PacketDotNet;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    [NotifyPropertyChanged]
    public class ConversationsStatisticsVm : DetectiveDataEntityViewModelBase
    {
        private ConversationsStatistics _conversationsStatistics;
        private const int TransportProtocolsCount = 2;

        /// <summary>
        ///     Placeholder toprevent databinding exceptions
        /// </summary>
        /// <param name="applicationWindsorContainerntainer"></param>
        public ConversationsStatisticsVm(IWindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.ConversationsStatistics = new ConversationsStatistics();
        }

        //public ConversationsStatisticsVm(WindsorContainer investigationOrAppWindsorContainer, IConversationsModel model) : base(investigationOrAppWindsorContainer, model)
        //{
        //    try  {
        //        Task.Run(() => this.InitializeAsync(model));
        //    }
        //    catch(Exception exception) {
        //        SystemMessage.SendSystemMessage(SystemMessage.Type.Error, "ConversationsStatistics Exception", exception.Message, this.GetType());
        //    }
        //}

        public ConversationsStatistics ConversationsStatistics
        {
            get { return this._conversationsStatistics; }
            private set
            {
                this._conversationsStatistics = value; 
                this.OnPropertyChanged();
            }
        }

        public async Task InitializeAsync(IConversationsModel model)
        {
            if(this.Initialized) return;
            this.Initialized = true;

            await Task.Run(() => { this.Initialize(model); });

        }

        private void Initialize(IConversationsModel model)
        {
            //var conversationsStatistics = new ConversationsStatistics
            //{
            //    Frames = model.Frames.Count(),
            //    CaptureStart = DateTime.MaxValue,
            //    CaptureFinish = DateTime.MinValue
            //};

            this.ConversationsStatistics.Frames = model.Frames.Count();
            this.ConversationsStatistics.CaptureStart = DateTime.MaxValue;
            this.ConversationsStatistics.CaptureFinish = DateTime.MinValue;

            this.ComputeConversationStatistics(model);
        }

        private void ComputeConversationStatistics(IConversationsModel model)
        {
            var hosts = new List<IPAddress>();
            var protoDistribution = new Dictionary<string, long>();
            var missingBytesListNew = new SortedList<DateTime, KeyValue<DateTime, long>>();
            var historyListNew = new SortedList<DateTime, KeyValue<DateTime, long>>();
            var hostsTraffic = new Dictionary<IPAddress, HostTraffic>();
            var avg = (long) this.Average(model.L7Conversations, conversation => conversation.ConversationStats.Bytes);
            foreach(var l7Conversation in model.L7Conversations)
            {
                if(!l7Conversation.L7PDUs.Any())
                {
                    // TODO: Is this correct?
                    continue;
                }

                // TO L3 foreach
                if(this.ConversationsStatistics.CaptureStart > l7Conversation.FirstSeen.ToUniversalTime()) {
                    this.ConversationsStatistics.CaptureStart = l7Conversation.FirstSeen.ToUniversalTime();
                }
                if(this.ConversationsStatistics.CaptureFinish < l7Conversation.LastSeen.ToUniversalTime()) {
                    this.ConversationsStatistics.CaptureFinish = l7Conversation.LastSeen.ToUniversalTime();
                }
                switch(l7Conversation.L3ProtocolType)
                {
                    case AddressFamily.InterNetwork:
                        this.ConversationsStatistics.IPv4Conversations++;
                        break;
                    case AddressFamily.InterNetworkV6:
                        this.ConversationsStatistics.IPv6Conversations++;
                        break;
                }
                hosts.Add(l7Conversation.SourceEndPoint.Address);
                hosts.Add(l7Conversation.DestinationEndPoint.Address);

                // To L4 foreach
                switch(l7Conversation.L4ProtocolType)
                {
                    case IPProtocolType.TCP:
                        this.ConversationsStatistics.TcpConversations++;
                        this.ConversationsStatistics.UpFlowTcpLostBytes += l7Conversation.UpConversationStatistic?.MissingBytes ?? 0;
                        this.ConversationsStatistics.DownFlowTcpLostBytes += l7Conversation.DownConversationStatistic?.MissingBytes ?? 0;
                        this.ConversationsStatistics.TotalTcpBytes += ((long?)l7Conversation.ConversationStats?.Bytes) ?? 0;
                        break;
                    case IPProtocolType.UDP:
                        this.ConversationsStatistics.UdpConversations++;
                        this.ConversationsStatistics.TotalUdpBytes += ((long?)l7Conversation.ConversationStats?.Bytes) ?? 0;
                        break;
                }

                // TO L7 statistics
                this.ConversationsStatistics.UpFlowFrames += l7Conversation.UpConversationStatistic?.Frames ?? 0;
                this.ConversationsStatistics.DownFlowFrames += l7Conversation.DownConversationStatistic?.Frames ?? 0;
                this.ConversationsStatistics.UpFlowBytes += l7Conversation.UpConversationStatistic?.Bytes ?? 0;
                this.ConversationsStatistics.DownFlowBytes += l7Conversation.DownConversationStatistic?.Bytes ?? 0;

                //TODO how this should be implemented? What happens if there are more app tags at once
                string apptag;
                if(l7Conversation.ApplicationTags.IsNullOrEmpty()) {
                    apptag = "unknown";
                }
                else if(l7Conversation.ApplicationTags.Count() == 1)
                {
                    apptag = l7Conversation.ApplicationTags.First();
                    if(apptag.IsNullOrEmpty()) apptag = "unknown";
                }
                else
                {
                    apptag = "multiple-protocols";
                }

                if (l7Conversation.ConversationStats == null) continue;
                
                    if(!protoDistribution.ContainsKey(apptag)) { protoDistribution.Add(apptag, l7Conversation.ConversationStats.Bytes); }
                    else
                    { protoDistribution[apptag] += l7Conversation.ConversationStats.Bytes; }
                

                // Traffic errors and history
                if(l7Conversation.FirstSeen != DateTime.MinValue && l7Conversation.FirstSeen != DateTime.MaxValue)
                {
                    var ct = l7Conversation.FirstSeen.ToUniversalTime();

                    KeyValue<DateTime, long> tmp;
                    if(!missingBytesListNew.TryGetValue(ct, out tmp))
                    {
                        tmp = new KeyValue<DateTime, long>(ct, l7Conversation.ConversationStats.MissingBytes);
                        missingBytesListNew.Add(ct, tmp);
                    }
                    else
                    {
                        tmp.Value += l7Conversation.ConversationStats.MissingBytes;
                    }
                    if(!historyListNew.TryGetValue(ct, out tmp))
                    {
                        tmp = new KeyValue<DateTime, long>(ct, l7Conversation.ConversationStats.Bytes);
                        historyListNew.Add(ct, tmp);
                    }
                    else
                    {
                        tmp.Value += l7Conversation.ConversationStats.Bytes;
                    }
                }

                // hosts traffic
                if(l7Conversation.ConversationStats.Bytes < avg)
                {
                    continue;
                }

                var srcHostAddress = l7Conversation.SourceEndPoint.Address;
                HostTraffic srcHost;
                if(!hostsTraffic.TryGetValue(srcHostAddress, out srcHost))
                {
                    srcHost = new HostTraffic
                    {
                        Host = srcHostAddress
                    };
                    hostsTraffic.Add(srcHostAddress, srcHost);
                }

                var dstHostAddress = l7Conversation.DestinationEndPoint.Address;
                HostTraffic dstHost;
                if(!hostsTraffic.TryGetValue(dstHostAddress, out dstHost))
                {
                    dstHost = new HostTraffic
                    {
                        Host = dstHostAddress
                    };
                    hostsTraffic.Add(dstHostAddress, dstHost);
                }

                srcHost.UpTraffic += l7Conversation.UpConversationStatistic.Bytes;
                srcHost.DownTraffic += l7Conversation.DownConversationStatistic.Bytes;
                dstHost.UpTraffic += l7Conversation.DownConversationStatistic.Bytes;
                dstHost.DownTraffic += l7Conversation.UpConversationStatistic.Bytes;
            }

            this.ConversationsStatistics.UniqueHostsCount = (uint) hosts.Distinct().Count();

            //TODO: hack, Cant bind KeyValuePair (value type)
            // this.ConversationsStatistics.AppProtocolsDistribution = protoDistribution.ToArray();
            this.ConversationsStatistics.ReckognizedProtocolsCount = (uint) protoDistribution.Count();
            this.ConversationsStatistics.AppProtocolsDistribution = new KeyValue<string, long>[this.ConversationsStatistics.ReckognizedProtocolsCount];
            this.ConversationsStatistics.AppProtocolsSummary = new ProtocolSummaryItem[this.ConversationsStatistics.ReckognizedProtocolsCount];
            var i = 0;
            foreach(var proto in protoDistribution)
            {
                this.ConversationsStatistics.AppProtocolsDistribution[i] = new KeyValue<string, long>(proto.Key, proto.Value);
                this.ConversationsStatistics.AppProtocolsSummary[i] = new ProtocolSummaryItem(proto.Key, proto.Value,
                    (float) proto.Value / this.ConversationsStatistics.TotalFlowBytes * 100);
                i++;
            }

            this.ConversationsStatistics.TransportProtocolsDistribution = new KeyValue<string, long>[TransportProtocolsCount];
            this.ConversationsStatistics.TransportProtocolsSummary = new ProtocolSummaryItem[TransportProtocolsCount];
            this.ConversationsStatistics.TransportProtocolsDistribution[0] = new KeyValue<string, long>("TCP", this.ConversationsStatistics.TotalTcpBytes);
            this.ConversationsStatistics.TransportProtocolsDistribution[1] = new KeyValue<string, long>("UDP", this.ConversationsStatistics.TotalUdpBytes);
            var tcpPerc = (float) this.ConversationsStatistics.TotalTcpBytes / (this.ConversationsStatistics.TotalTcpBytes + this.ConversationsStatistics.TotalUdpBytes) * 100;
            var udpPerc = (float) this.ConversationsStatistics.TotalUdpBytes / (this.ConversationsStatistics.TotalTcpBytes + this.ConversationsStatistics.TotalUdpBytes) * 100;
            this.ConversationsStatistics.TransportProtocolsSummary[0] = new ProtocolSummaryItem("TCP", this.ConversationsStatistics.TotalTcpBytes, tcpPerc);
            this.ConversationsStatistics.TransportProtocolsSummary[1] = new ProtocolSummaryItem("UDP", this.ConversationsStatistics.TotalUdpBytes, udpPerc);

            // traffic errors
            this.ConversationsStatistics.TrafficErrors = missingBytesListNew.Values.ToArray();

            // history
            this.ConversationsStatistics.TrafficHistory = historyListNew.Values.ToArray();

            // hosts traffic
            this.ConversationsStatistics.HostsTraffic = hostsTraffic.Values.OrderByDescending(h => h.TotalTraffic).ToArray();
        }

        /// <summary>
        ///     http://proveitwithaunittest.wordpress.com/2013/07/19/c-using-parallel-linq-plinq-to-find-the-average/
        ///     http://stackoverflow.com/questions/5075484/property-selector-expressionfunct-how-to-get-set-value-to-selected-property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private double Average<T>(IEnumerable<T> items, Expression<Func<T, long>> selector)
        {
            var sel = selector.Compile();
            return items.AsParallel().Aggregate(
                // the datetype its starting with this will be used for TSource and TAccumulate
                new AverageStruct(),
                // This section is repeated multiple times on multiple threads.
                // subTotal is passed from call to call on a thread, each call
                // gives the next item from items. So we are merging item into subTotal
                (subTotal, item) =>
                {
                    if(item != null)
                    {
                        subTotal.Sum += sel(item);
                        subTotal.Count++;
                    }
                    return subTotal;
                },
                // This section happens after a single thread has completed its
                // assigned number of items from items IEnumerable.
                // its job is to take total an AverageStruct and to take thisThread also an AverageStruct
                // and then to merge them together. So the end result is one AverageStruct item after all
                // the threads have completed their processing.
                (total, thisThread) =>
                {
                    total.Sum += thisThread.Sum;
                    total.Count += thisThread.Count;
                    return total;
                },
                // This is the simple average calculation. final is that overal merged AverageStruct
                // which contains all the needed answers to make the final calulation.
                // The casting final.Count to double is needed because other wise
                // since both values and ints they will divide like ints.
                final => final.Sum/(double) final.Count);
        }

        private struct AverageStruct
        {
            // need Int64 because otherwise you end up with negative numbers when the bit flips
            public Int64 Sum { get; set; }
            public Int64 Count { get; set; }
        }
    }
}