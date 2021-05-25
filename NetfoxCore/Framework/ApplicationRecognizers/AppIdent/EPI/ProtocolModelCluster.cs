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
using System.Linq;
using Castle.Core.Internal;
using Numl.Unsupervised;
using Netfox.AppIdent.Models;
using Netfox.AppIdent.Statistics;

namespace Netfox.AppIdent.EPI
{
    public class ProtocolModelCluster : Cluster
    {
        /// <summary>Default constructor.</summary>
        public ProtocolModelCluster(Cluster cluster)
        {
            this.Id = cluster.Id;
            this.Center = cluster.Center;
            this.Members = cluster.Members.Cast<FeatureVector>().ToArray();

            this.Children = cluster.Children?.Select(child => new ProtocolModelCluster(child)).ToArray();
            this.FlattenChildren = this.Children?.SelectMany(child => child?.FlattenChildren).ToArray() ?? new[]
            {
                this
            };
            this.FlattenChildrenAppTags = this.FlattenChildren.SelectMany(protocolModelCluster => protocolModelCluster.FlattenChildrenAppTags ?? new[]
            {
                protocolModelCluster.ClusterAppTags
            }).ToArray();
        }

        public ApplicationProtocolClassificationStatistics ApplicationProtocolClassificationStatistics { get; set; }

        public new ProtocolModelCluster[] Children { get; set; }
        public ProtocolModelCluster[] FlattenChildren { get; set; }
        public string[] FlattenChildrenAppTags { get; set; }
        public new FeatureVector[] Members { get; set; }

        public string ClusterAppTags => string.Join(", ", this.Members.OfType<FeatureVector>().Select(m => m.Label));

        public void UpdateStatistics(ApplicationProtocolClassificationStatisticsMeter applicationProtocolClassificationStatisticsMeter)
        {
            if(!this.Children.IsNullOrEmpty())
            {
                foreach(var cluster in this.Children) //drill down
                {
                    cluster.UpdateStatistics(applicationProtocolClassificationStatisticsMeter);
                }

                this.ApplicationProtocolClassificationStatistics = this.RecalculateApplicationProtocolClassificationStatisticsForCluster();
            }
            else { this.ApplicationProtocolClassificationStatistics = applicationProtocolClassificationStatisticsMeter[this.Members.First().Label]; }
        }

        private ApplicationProtocolClassificationStatistics RecalculateApplicationProtocolClassificationStatisticsForCluster()
        {
            if(this.FlattenChildren == null) return this.ApplicationProtocolClassificationStatistics;

            var clusterAppProtoClassStats = new ApplicationProtocolClassificationStatistics(this);

            foreach(var children in this.FlattenChildren)
            {
                if(children.ApplicationProtocolClassificationStatistics == null)
                {
                    continue; //TODO check, why is sometines null
                }
                var childrenStats = children.ApplicationProtocolClassificationStatistics;

                //TP
                clusterAppProtoClassStats.TP += childrenStats.TP;

                //FP
                foreach(var fpStat in childrenStats.FPsStatistics)
                {
                    if(this.FlattenChildrenAppTags.Contains(fpStat.Key, StringComparer.InvariantCultureIgnoreCase)) clusterAppProtoClassStats.TP += fpStat.Value;
                    else { clusterAppProtoClassStats.AddFP(fpStat.Key); }
                }

                //FN
                foreach(var fnStat in childrenStats.FNsStatistics)
                {
                    if(this.FlattenChildrenAppTags.Contains(fnStat.Key, StringComparer.InvariantCultureIgnoreCase)) clusterAppProtoClassStats.TP += fnStat.Value;
                    else { clusterAppProtoClassStats.AddFN(fnStat.Key); }
                }
            }
            return clusterAppProtoClassStats;
        }
    }
}