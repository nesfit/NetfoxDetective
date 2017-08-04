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
using System.Text;
using Castle.Core.Internal;
using Newtonsoft.Json;

namespace Netfox.AnalyzerSIPFraud.Models
{
    public class JsonModels
    {
        public class Stats
        {
            [JsonProperty(PropertyName = "caller-count")]
            public int CallerCount { get; set; }
            [JsonProperty(PropertyName = "callee-count")]
            public int CalleeCount { get; set; }
            [JsonProperty(PropertyName = "invite-count")]
            public int InviteCount { get; set; }
            [JsonProperty(PropertyName = "calls-per-caller")]
            public int CallsPerCaller { get; set; }

            #region Overrides of Object
            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("caller-count: ");
                sb.Append(this.CallerCount);

                sb.Append(", callee-count: ");
                sb.Append(this.CalleeCount);

                sb.Append(", invite-count: ");
                sb.Append(this.InviteCount);

                sb.Append(", caller-count: ");
                sb.Append(this.CallerCount);

                sb.Append(", calls-per-caller: ");
                sb.Append(this.CallsPerCaller);

                return sb.ToString();
            }
            #endregion
        }

        public class Message
        {
            [JsonProperty(PropertyName = "stats")]
            public Stats Stats { get; set; }
            [JsonProperty(PropertyName = "suspicious-host")]
            public string SuspiciousHost { get; set; }
            [JsonProperty(PropertyName = "progress")]
            public int Progress { get; set; }
            [JsonProperty(PropertyName = "files")]
            public List<string> Files { get; set; }

            public string FilesString => this.Files.IsNullOrEmpty()?"": String.Join(",\n", this.Files);
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

   
    }
}
