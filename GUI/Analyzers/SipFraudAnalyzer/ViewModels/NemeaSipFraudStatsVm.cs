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

using System.Text;
using GalaSoft.MvvmLight;
using Netfox.AnalyzerSIPFraud.Models;
using Netfox.Core.Collections;

namespace Netfox.AnalyzerSIPFraud.ViewModels
{
    public class NemeaSipFraudStatsVm : ViewModelBase
    {
        public NemeaSipFraudStatsVm(JsonModels.Stats stats) { this.Stats = stats; }
        private JsonModels.Stats Stats { get; }

        public int CallerCount => this.Stats.CallerCount;
        public int CalleeCount => this.Stats.CallerCount;
        public int InviteCount => this.Stats.CallerCount;
        public int CallsPerCaller => this.Stats.CallerCount;
        
        public ConcurrentObservableCollection<string> SuspiciousIPs { get; } = new ConcurrentObservableCollection<string>();

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
}