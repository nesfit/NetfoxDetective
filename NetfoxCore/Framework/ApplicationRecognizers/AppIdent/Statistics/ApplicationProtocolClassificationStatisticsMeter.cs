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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Netfox.AppIdent.Statistics
{
    [DataContract]
    public class ApplicationProtocolClassificationStatisticsMeter
    {
        [DataMember]
        public ConcurrentDictionary<string, ApplicationProtocolClassificationStatistics> AppStatistics { get; set; } =
            new ConcurrentDictionary<string, ApplicationProtocolClassificationStatistics>(StringComparer.OrdinalIgnoreCase);
        
        public ApplicationProtocolClassificationStatistics this[string appTag]
        {
            get
            {
                ApplicationProtocolClassificationStatistics stats;
                this.AppStatistics.TryGetValue(appTag, out stats);
                return stats;
            }
        }

        public void PrintResults() { Console.WriteLine(this.ToString()); }

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
            foreach(var applicationProtocolPrecision in this.AppStatistics.OrderBy(KeyValuePair => KeyValuePair.Key).Select(kvp => kvp.Value)
                .OrderByDescending(appStat => appStat.TP)) { sb.AppendLine(applicationProtocolPrecision.ToString()); }
            return sb.ToString();
        }
        #endregion

        public void UpdateStatistics(string predictedAppTag, string appTag)
        {
            predictedAppTag = predictedAppTag?.ToLower();
            appTag = appTag?.ToLower();
            if(appTag == null || predictedAppTag == null) { return; }

            if(!this.AppStatistics.ContainsKey(predictedAppTag)) { this.AppStatistics.TryAdd(predictedAppTag, new ApplicationProtocolClassificationStatistics(predictedAppTag)); }

            if(!this.AppStatistics.ContainsKey(appTag)) { this.AppStatistics.TryAdd(appTag, new ApplicationProtocolClassificationStatistics(appTag)); }

            if(appTag == predictedAppTag) { this.AppStatistics[predictedAppTag].AddTP(predictedAppTag); }
            else if(appTag != predictedAppTag)
            {
                this.AppStatistics[predictedAppTag].AddFP(appTag);
                this.AppStatistics[appTag].AddFN(predictedAppTag);
            }
        }

        public void SaveToCsv(string csvFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("sep=;");
            sb.AppendLine($"PredictedAppTag;TP;FP;FN;Precission;Recall;FMeasure;");
            foreach (var appStat in this.AppStatistics.Values)
            {
                sb.AppendLine($"{appStat.PredictedAppTag.Replace(",", " ")};{appStat.TP};{appStat.FP};{appStat.FN};{appStat.Precission};{appStat.Recall};{appStat.FMeasure};");
            }
            var csv = sb.ToString();
            using(Stream myStream = new FileStream(csvFilePath,FileMode.Create))
            using(var sw = new StreamWriter(myStream, Encoding.UTF8))
            {
                sw.Write(csv);
                sw.Flush(); //otherwise you are risking empty stream
                myStream.Seek(0, SeekOrigin.Begin);
            }

        }
    }
}