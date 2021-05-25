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
using System.Text;
using System.Threading.Tasks;
using Netfox.AppIdent.Features.Bases;
using Netfox.AppIdent.Models;
using Netfox.Framework.Models;

namespace Netfox.AppIdent.EPI
{
    public class AppIdentDataSource
    {
        public FeatureVector[] TrainingSet { get; set; }
        public FeatureVector[] VerificationSet { get; set; }

        public List<AppIdentDataSourceStats> AppIdentDataSourceStatistics { get; } = new List<AppIdentDataSourceStats>();
        private ConcurrentBag<FeatureVector> FeatureVectors { get; set; } = new ConcurrentBag<FeatureVector>();

        public void Initialize(IEnumerable<L7Conversation> l7Conversations, int minFlows = 1, double trainingToClassifyingRatio = 1)
        {
            if(this.FeatureVectors == null) { throw new InvalidOperationException($"{nameof(this.FeatureVectors)} is null!"); }


            var tmpListOfConversations = l7Conversations.Where(c => c.AppTag != null).ToArray();
            var groupedConverstions = from conversation in tmpListOfConversations
                group conversation by conversation.AppTag
                into groupedConverstion
                orderby groupedConverstion.Key
                select groupedConverstion;
            foreach(var gc in groupedConverstions.Where(convs => convs.Count() > minFlows))
            {
                Parallel.ForEach(gc, l7Conv =>
                {
                    var applicationName = l7Conv.AppTag;
                    if(applicationName == null) { return; }
                    this.FeatureVectors.Add(this.ComputeFeatureVector(l7Conv, applicationName));
                });
            }

            if(Math.Abs(trainingToClassifyingRatio - 1) < 0.01) { this.AllInTraining(); }
            else { this.PartitionFeatureVectorsTestingAndVerificationDatasets(trainingToClassifyingRatio, minFlows); }
        }

        public void RepartitionFeatureVectorsTestingAndVerificationDatasets(double trainingToClassifyingRatio, int minTrainingFlows)
        {
            IEnumerable<FeatureVector> data = this.TrainingSet;
            if(this.VerificationSet != null && this.VerificationSet.Any()) data = data.Concat(this.VerificationSet);
            this.FeatureVectors = new ConcurrentBag<FeatureVector>(data);

            this.TrainingSet = null;
            this.VerificationSet = null;

            this.PartitionFeatureVectorsTestingAndVerificationDatasets(trainingToClassifyingRatio, minTrainingFlows);
        }

        private void AllInTraining()
        {
            if(this.TrainingSet != null || this.VerificationSet != null) throw new NotSupportedException("Partitioning is one time only operation!");
            this.TrainingSet = this.FeatureVectors.ToArray();
        }

        private FeatureVector ComputeFeatureVector(L7Conversation l7Conversation, string applicationName)
        {
            var featureVector = new FeatureVector
            {
                ApplicationProtocolName = l7Conversation.AppTagShort,
                ApplicationProtocolNameFull = applicationName
            };

            Parallel.ForEach(featureVector.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))),
                feature => { feature.SetValue(featureVector, Activator.CreateInstance(feature.PropertyType, l7Conversation)); });
            return featureVector;
        }

        private void PartitionFeatureVectorsTestingAndVerificationDatasets(double trainingToClassifyingRatio, int minTrainingFlows)
        {
            if(this.TrainingSet != null || this.VerificationSet != null) throw new NotSupportedException("Partitioning is one time only operation!");

            var trainingSet = new List<FeatureVector>();
            var verificationSet = new List<FeatureVector>();

            var groupedFeatureVectors = from featureVector in this.FeatureVectors
                group featureVector by featureVector.Label
                into featureVectors
                orderby featureVectors.Key
                select featureVectors;

            var rand = new Random(0);
            foreach(var gc in groupedFeatureVectors)
            {
                var conves = gc.ToList();
                var cTraining = new List<FeatureVector>();
                var cVerification = new List<FeatureVector>();
                foreach(var c in conves)
                {
                    if(rand.NextDouble() <= trainingToClassifyingRatio) { cTraining.Add(c); }
                    else { cVerification.Add(c); }
                }
                while(cTraining.Count < minTrainingFlows)
                {
                    if(cVerification.Count == 0)
                    {
                        cTraining.Clear();
                        cVerification.Clear();
                        break;
                    }
                    var index = rand.Next(0, cVerification.Count);
                    var swapedFeatureVector = cVerification[index];
                    cVerification.RemoveAt(index);
                    cTraining.Add(swapedFeatureVector);
                }
                if(cTraining.Count != 0)
                {
                    trainingSet.AddRange(cTraining);
                    verificationSet.AddRange(cVerification);
                    this.AppIdentDataSourceStatistics.Add(new AppIdentDataSourceStats(gc.Key, cTraining.Count, cVerification.Count));
                }
            }

            this.TrainingSet = trainingSet.ToArray();
            this.VerificationSet = verificationSet.ToArray();
        }

        public void SaveToCsv(string csvFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("sep=;");
            sb.AppendLine($"Label;Training;Verification");
            foreach (var stat in this.AppIdentDataSourceStatistics)
            {
                sb.AppendLine($"{stat.Label};{stat.Training};{stat.Verification};");
            }
            var csv = sb.ToString();
            using (Stream myStream = new FileStream(csvFilePath, FileMode.Create))
            using (var sw = new StreamWriter(myStream, Encoding.UTF8))
            {
                sw.Write(csv);
                sw.Flush(); //otherwise you are risking empty stream
                myStream.Seek(0, SeekOrigin.Begin);
            }
        }

        public class AppIdentDataSourceStats
        {
            public AppIdentDataSourceStats(string label, int training, int verification)
            {
                this.Label = label;
                this.Training = training;
                this.Verification = verification;
            }

            public string Label { get; set; }
            public int Training { get; set; }
            public int Verification { get; set; }
        }


    }
}