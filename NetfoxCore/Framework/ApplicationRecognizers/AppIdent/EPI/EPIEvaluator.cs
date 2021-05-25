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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Numl.Math.Linkers;
using Numl.Math.Metrics;
using Numl.Model;
using Numl.Unsupervised;
using Numl.Utils;
using Netfox.AppIdent.Accord;
using Netfox.AppIdent.Models;
using Netfox.AppIdent.Statistics;
using Netfox.Core.Properties;

//using Catharsis.Commons;

namespace Netfox.AppIdent.EPI
{
    public class EPIEvaluator : INotifyPropertyChanged
    {
        public FeatureSelector FeatureSelector { get; }
        private ObservableCollection<EPIProtocolModel> _protocolModels;
        public EPIEvaluator(FeatureSelector featureSelector) { this.FeatureSelector = featureSelector; }

        public ObservableCollection<EPIProtocolModel> ProtocolModels
        {
            get => this._protocolModels;
            set
            {
                if(Equals(value, this._protocolModels)) return;
                this._protocolModels = value;
                this.OnPropertyChanged();
            }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        public void AgregateProtocolModels()
        {
            var differences = new List<Tuple<EPIProtocolModel, EPIProtocolModel, double>>();
            foreach(var protocolModelX in this.ProtocolModels)
            {
                foreach(var protocolModelY in this.ProtocolModels)
                {
                    if(protocolModelX == protocolModelY) continue;
                    var diff = protocolModelX.ComputeDistanceToProtocolModel(protocolModelY);
                    differences.Add(new Tuple<EPIProtocolModel, EPIProtocolModel, double>(protocolModelX, protocolModelY, diff));
                }
            }
            var differencesVector = differences.GroupBy(d => d.Item1).Select(tuples => new
            {
                protocol = tuples.Key,
                protocolDiffs = tuples.Select(t => new
                {
                    proto = t.Item2,
                    diff = t.Item3
                }),
                diffs = tuples.Select(t => t.Item3)
            });
            foreach(var modelDifference in differencesVector)
            {
                var stddev = modelDifference.diffs.StandardDeviation(d => d);
                var mean = modelDifference.diffs.Average(d => d);
                var min = modelDifference.diffs.Min();
                var max = modelDifference.diffs.Max();
                Console.WriteLine($"Name:{modelDifference.protocol.ApplicationProtocolName}, MIN: {min}, MAX: {max}, Mean: {mean}, StdDev: {stddev}");
                foreach(var diff in modelDifference.protocolDiffs.OrderBy(d => d.diff).ThenBy(d => d.proto.ApplicationProtocolName))
                {
                    Console.WriteLine($"{diff.proto.ApplicationProtocolName} {diff.diff}");
                }
            }

            //foreach (var diff in differences.OrderBy(d => d.Item1.ApplicationProtocolName).ThenBy(d => d.Item3)) {
            //    Console.WriteLine($"{diff.Item1.ApplicationProtocolName} {diff.Item2.ApplicationProtocolName} {diff.Item3}");
            //}
        }

        #region statistics
        public ApplicationProtocolClassificationStatisticsMeter ComputeStatistics(IEnumerable<FeatureVector> verificationSet)
        {
            var precMeasure = new ApplicationProtocolClassificationStatisticsMeter();
            foreach(var featureVector in verificationSet)
            {
                var appTag = featureVector.Label;
                var predicted = this.Predict(featureVector);
                precMeasure.UpdateStatistics(predicted, appTag);
            }
            return precMeasure;
        }
        #endregion

        public void CreateApplicationProtocolModels(IEnumerable<FeatureVector> featureVectors)
        {
            if(this.ProtocolModels != null) { throw new InvalidOperationException($"{nameof(this.ProtocolModels)} have already been computed!"); }

            var trainingFeatureVectorsArray = featureVectors.ToArray();
            var procolModels = new ConcurrentDictionary<string, EPIProtocolModel>();
            Parallel.ForEach(trainingFeatureVectorsArray, trainingFeatureVector =>
            {
                var applicationName = trainingFeatureVector.Label;
                procolModels.AddOrUpdate(applicationName, new EPIProtocolModel(applicationName, trainingFeatureVector,this.FeatureSelector), (s, model) =>
                {
                    model.AddTrainingFeatureVector(trainingFeatureVector);
                    return model;
                });
            });
            this.ProtocolModels = new ObservableCollection<EPIProtocolModel>(procolModels.Values);
            Parallel.ForEach(this.ProtocolModels, model => model.ComputeApplicationProtocolModel());
        }

        public void LoadProtocolModelsFromXml(string fileName)
        {
            try
            {
                var dcs = new DataContractSerializer(typeof(EPIProtocolModel[]));
                var fs = new FileStream(fileName, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                this.ProtocolModels = new ObservableCollection<EPIProtocolModel>((EPIProtocolModel[]) dcs.ReadObject(reader));
                reader.Close();
                fs.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot Load model " + fileName);
                Console.WriteLine(ex);
                throw;
            }

            Parallel.ForEach(this.ProtocolModels, model => model.SetFeatureVectorProperties());
        }

        //TODO check implementation
        public string Predict(FeatureVector featureVector)
        {
            foreach(var model in this.ProtocolModels)
            {
                try
                {
                    var diff = model.ComputeDistanceToProtocolModelValue(featureVector);
                    if(diff == 0) { diff = Double.PositiveInfinity; }
                    featureVector.ProtocolModelDifferences.Add(model, diff);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    Debugger.Break();
                    return null;
                }
            }
            return featureVector.PredictedModel.ApplicationProtocolName;
        }
        
        public void PrintCsvProtocolModels()
        {
            Console.WriteLine(this.ProtocolModels.FirstOrDefault().ToCsvHeaderString());
            foreach(var protocolModel in this.ProtocolModels.OrderBy(m => m.ApplicationProtocolName)) { Console.WriteLine(protocolModel.ToCsvString()); }
        }

        public void PrintProtocolModels()
        {
            foreach(var protocolModel in this.ProtocolModels.OrderBy(m => m.ApplicationProtocolName)) { Console.WriteLine(protocolModel.ToString()); }
        }

        public void SaveProtocolModelsToXml(string fileName)
        {
            try
            {
                var serializer = new DataContractSerializer(typeof(EPIProtocolModel[]));
                var writer = XmlWriter.Create(fileName);
                serializer.WriteObject(writer, this.ProtocolModels);
                writer.Close();
            }
            catch(Exception ex) { Console.WriteLine("EXCEPTION during serialization\n" + ex); }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Clustering
        public IEnumerable<ProtocolModelCluster> ApplicationProtocolModelsHierachivalClustering()
        {
            return this.HierarchicalClustering(new AverageLinker(new EuclidianDistance()));

            //TODO Experiment with following configutaions
            //this.HierarchicalClustering(new CentroidLinker(new CosineDistance()));
            //this.HierarchicalClustering(new CentroidLinker(new EuclidianDistance()));
            //this.HierarchicalClustering(new CentroidLinker(new HammingDistance()));
            //this.HierarchicalClustering(new CentroidLinker(new ManhattanDistance()));

            //this.HierarchicalClustering(new AverageLinker(new CosineDistance()));
            //this.HierarchicalClustering(new AverageLinker(new CosineDistance()));
            //this.HierarchicalClustering(new AverageLinker(new HammingDistance()));
            //this.HierarchicalClustering(new AverageLinker(new ManhattanDistance()));

            //this.HierarchicalClustering(new CompleteLinker(new CosineDistance()));
            //this.HierarchicalClustering(new CompleteLinker(new EuclidianDistance()));
            //this.HierarchicalClustering(new CompleteLinker(new HammingDistance()));
            //this.HierarchicalClustering(new CompleteLinker(new ManhattanDistance()));

            //this.HierarchicalClustering(new SingleLinker(new CosineDistance()));
            //this.HierarchicalClustering(new SingleLinker(new EuclidianDistance()));
            //this.HierarchicalClustering(new SingleLinker(new HammingDistance()));
            //this.HierarchicalClustering(new SingleLinker(new ManhattanDistance()));
        }

        private IEnumerable<ProtocolModelCluster> HierarchicalClustering(ILinker linker)
        {
            var model = new HClusterModel();
            var desc = Descriptor.Create<FeatureVector>();

            var l4ProtocolsGroups = this.ProtocolModels?.Select(pm => pm.ModelFeatureVector).GroupBy(mfv => mfv.TransportProtocolType.L4ProtocolType).Select(kvp => new
            {
                l4ProtocolType = kvp.Key,
                featureVectors = kvp.Select(fv => fv)
            });

            var masterClusterChildrens = l4ProtocolsGroups?.Select(l4ProtocolsGroup => model.Generate(desc, l4ProtocolsGroup.featureVectors, linker));
            return masterClusterChildrens?.Select(cluster => new ProtocolModelCluster(cluster)).ToList();
        }

        public void PrintClusters(IEnumerable<ProtocolModelCluster> clusters = null)
        {
            foreach(var cluster in clusters ?? this.ApplicationProtocolModelsHierachivalClustering()) { this.PrintCluster(cluster, 0); }
        }

        private void PrintCluster(ProtocolModelCluster cluster, int indent = 0)
        {
            this.PrintClusterGroupWithIndent(string.Join(", ", cluster.Members.OfType<FeatureVector>().Select(m => m.Label)), indent);

            if(cluster.Children != null && cluster.Children.Any())
            {
                foreach(var child in cluster.Children)
                {
                    this.PrintCluster(child, indent + 1); // Increase the indent for children
                }
            }
        }

        private void PrintClusterGroupWithIndent(string value, int indent) { Console.WriteLine("{0}{1}", new string(' ', indent * 2), value); }
        #endregion
    }
}