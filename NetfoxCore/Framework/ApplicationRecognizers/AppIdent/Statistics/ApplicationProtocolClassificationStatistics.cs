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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Netfox.AppIdent.EPI;

namespace Netfox.AppIdent.Statistics
{
    [DataContract]
    public class ApplicationProtocolClassificationStatistics
    {
        [DataMember] public Dictionary<string, int> FNsStatistics = new Dictionary<string, int>();

        [DataMember] public Dictionary<string, int> FPsStatistics = new Dictionary<string, int>();

        public ApplicationProtocolClassificationStatistics(string predictedAppTag) { this.PredictedAppTag = predictedAppTag; }

        public ApplicationProtocolClassificationStatistics(ProtocolModelCluster protocolModelCluster) { this.PredictedAppTag = protocolModelCluster.ClusterAppTags; }

        [DataMember]
        public string PredictedAppTag { get; set; }

        [DataMember]
        public int TP { get; set; }

        [DataMember]
        public int FP { get; set; }

        [DataMember]
        public int FN { get; set; }

        public double Precission => Utilities.Precission(this.TP, this.FP);
        public double Recall => Utilities.Recall(this.TP, this.TP + this.FN);
        public double FMeasure => Utilities.F_Measure(this.TP, this.FP, this.TP + this.FN);
        

        public string StatisticSummary
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"TP:{this.TP}, FP:{this.FP}, FN:{this.FN}");
                sb.Append($"FPs: ");
                foreach(var fpStat in this.FPsStatistics) { sb.Append($"{fpStat.Key}: {fpStat.Value}, "); }
                sb.Append($"\nFNs: ");
                foreach(var fnStat in this.FNsStatistics) { sb.Append($"{fnStat.Key}: {fnStat.Value}, "); }
                sb.AppendLine($"\nPrecision: {this.Precission}");
                sb.AppendLine($"Recall: {this.Recall}");
                sb.AppendLine($"F-Measure: {this.FMeasure}");
                return sb.ToString();
            }
        }

        public void AddFN(string predictedAppTag)
        {
            this.FN++;
            if(this.FNsStatistics.ContainsKey(predictedAppTag)) { this.FNsStatistics[predictedAppTag]++; }
            else { this.FNsStatistics.Add(predictedAppTag, 1); }
        }

        public void AddFP(string predictedAppTag)
        {
            this.FP++;
            if(this.FPsStatistics.ContainsKey(predictedAppTag)) { this.FPsStatistics[predictedAppTag]++; }
            else { this.FPsStatistics.Add(predictedAppTag, 1); }
        }

        public void AddTP(string predictedAppTag) { this.TP++; }

        #region Overrides of Object
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{this.PredictedAppTag}");
            sb.AppendLine(this.StatisticSummary);
            return sb.ToString();
        }
        #endregion
    }
}