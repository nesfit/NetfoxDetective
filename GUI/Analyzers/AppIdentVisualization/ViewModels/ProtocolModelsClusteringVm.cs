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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.Core.Internal;
using GalaSoft.MvvmLight.CommandWpf;
using Netfox.AnalyzerAppIdent.Models;
using Netfox.AnalyzerAppIdent.Properties;
using Netfox.AppIdent.Statistics;

namespace Netfox.AnalyzerAppIdent.ViewModels
{
    public class ProtocolModelsClusteringVm : INotifyPropertyChanged
    {
        public AppIdentMainVm AppIdentMainVm { get; set; }
        private RelayCommand<ClusterNodeModel> _collapsNodeCommand;

        private RelayCommand<ClusterNodeModel> _expandNodeCommand;

        public ProtocolModelsClusteringVm(AppIdentMainVm appIdentMainVm)
        {
            this.AppIdentMainVm = appIdentMainVm;
            this.AppIdentMainVm.EpiPrecMeasureObservable.Subscribe(this.InitializeClusters);
        }

        public ObservableCollection<ClusterNodeModel> Nodes { get; } = new ObservableCollection<ClusterNodeModel>();
        public RelayCommand<ClusterNodeModel> ExpandNodeCommand => this._expandNodeCommand ?? (this._expandNodeCommand = new RelayCommand<ClusterNodeModel>(this.ExpandCluster));

        public RelayCommand<ClusterNodeModel> CollapsNodeCommand => this._collapsNodeCommand ?? (this._collapsNodeCommand = new RelayCommand<ClusterNodeModel>(this.CollapsCluster));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CollapsCluster(ClusterNodeModel clusterNodeModel)
        {
            // if(!clusterNodeModel.IsExpanded || clusterNodeModel.ParrentCluster == null) { return; }

            ClusterNodeModel clusterNode;
            if (clusterNodeModel.IsExpanded && !clusterNodeModel.IsLiefNode) { clusterNode = clusterNodeModel; }
            else { clusterNode = clusterNodeModel.ParrentCluster; }

            clusterNode.IsExpanded = false;
            clusterNode.IsLiefNode = true;

            var clusterChildren = clusterNode.Cluster.Children == null? clusterNode.Cluster.FlattenChildren:  clusterNode.Cluster.FlattenChildren.Concat(clusterNode.Cluster.Children);
            if(!clusterChildren.IsNullOrEmpty())
            {
                foreach(var node in this.Nodes.ToArray()) {
                    if(clusterChildren.Contains(node.Cluster)) { this.Nodes.Remove(node); }
                }
            }
            this.UpdatePrecMeter();
        }

        private void ExpandCluster(ClusterNodeModel clusterNodeModel)
        {
            if(clusterNodeModel.IsExpanded) { return; }
            clusterNodeModel.IsExpanded = true;
            clusterNodeModel.IsLiefNode = false;
           

            var clusterChildren = clusterNodeModel.Cluster.Children;
            if(!clusterChildren.IsNullOrEmpty())
            {
                var newClusterNodes = new List<ClusterNodeModel>();
                foreach(var cluster in clusterChildren)
                {
                    var clusterNode = new ClusterNodeModel
                    {
                        Cluster = cluster,
                        ParrentCluster = clusterNodeModel
                    };
                    this.Nodes.Add(clusterNode);
                    newClusterNodes.Add(clusterNode);
                }
                foreach(var clusterNode in newClusterNodes)
                {
                    clusterNode.Connections.Add(new ConnectionModel
                    {
                        Target = clusterNodeModel
                    });
                    clusterNodeModel.Connections.Add(new ConnectionModel
                    {
                        Target = clusterNode
                    });
                }
            }

            this.UpdatePrecMeter();
        }

        private void UpdatePrecMeter()
        {
            var applicationProtocolClassificationStatisticsMeter = new ApplicationProtocolClassificationStatisticsMeter();
            foreach(var node in this.Nodes.Where(n=>n.IsLiefNode))
            {
                applicationProtocolClassificationStatisticsMeter.AppStatistics.AddOrUpdate(node.Cluster.ClusterAppTags, node.Cluster.ApplicationProtocolClassificationStatistics,
                    (s, statistics) => statistics);
            }
            this.PrecMeasure = applicationProtocolClassificationStatisticsMeter;
            this.AppIdentMainVm.EpiPrecMeasure = applicationProtocolClassificationStatisticsMeter;
        }

        public ApplicationProtocolClassificationStatisticsMeter PrecMeasure { get; set; }

        private void InitializeClusters(ApplicationProtocolClassificationStatisticsMeter precMeasure)
        {
            if(this.PrecMeasure == precMeasure) return;
            var cluesters = this.AppIdentMainVm.AppProtoModelEval.ApplicationProtocolModelsHierachivalClustering();
            this.Nodes.Clear();

            if(cluesters == null) return;

            foreach(var cluster in cluesters)
            {
                cluster.UpdateStatistics(precMeasure);
                var clusterNode = new ClusterNodeModel
                {
                    Cluster = cluster
                };
                clusterNode.ParrentCluster = clusterNode;
                this.Nodes.Add(clusterNode);
            }
        }
    }
}