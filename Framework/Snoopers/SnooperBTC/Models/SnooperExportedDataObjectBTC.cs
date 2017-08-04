// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
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
using System.Linq;
using System.Text;
using Netfox.Core.Database.Wrappers;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperBTC.Interfaces;

namespace Netfox.SnooperBTC.Models
{

	public class SnooperExportedDataObjectBTC : SnooperExportedObjectBase
    {
		public SnooperExportedDataObjectBTCType Type { get; set; }
        public string TypeString
        {
            get
            {
                switch (this.Type)
                {
                    case SnooperExportedDataObjectBTCType.Tx:
                        return "Transaction";
                    case SnooperExportedDataObjectBTCType.Version_Verack:
                        return "Registration";
                    default:
                        return "";
                }
            }
        }
        public SnooperExportedDataObjectBTCState State { get; set; } = SnooperExportedDataObjectBTCState.Other;

        public string StateString
        {
            get
            {
                switch (this.State)
                {
                    case SnooperExportedDataObjectBTCState.VersionReceived:
                        return "Half handshake (only request)";
                    case SnooperExportedDataObjectBTCState.VerackReceived:
                        return "Half handshake (only response)";
                    case SnooperExportedDataObjectBTCState.VersionAndVerackReceived:
                        return "Full handshake";
                    case SnooperExportedDataObjectBTCState.Other:
                        return "N/A";
                    default:
                        return "ERROR";
                }
            }
        }
        public IPAddressEF ClientAddress { get; set; }
        public IPAddressEF ServerAddress { get; set; }
        public List<string> UserAgents { get; set; } = new List<string>();
	    public string UserAgentsString => string.Join(", ", this.UserAgents);

        private SnooperExportedDataObjectBTC() : base() { } //EF
        public SnooperExportedDataObjectBTC(SnooperExportBase exportBase) : base(exportBase) { this.ExportValidity = ExportValidity.Malformed; }

		public override string ToString()
		{
			var converted = new StringBuilder();
			converted.AppendLine("type: " + this.Type);
			converted.AppendLine("state: " + this.State);
			if(this.UserAgents.Any())
				converted.AppendLine("user-agents: " + string.Join(", ", this.UserAgents));
			switch (this.Type)
			{
				case SnooperExportedDataObjectBTCType.Version_Verack:
					converted.AppendLine("clientAddr: " + this.ClientAddress);
					converted.Append("serverAddr: " + this.ServerAddress);
					break;
				case SnooperExportedDataObjectBTCType.Tx:
					converted.AppendLine("clientAddr: " + this.ClientAddress);
					converted.Append("serverAddr: " + this.ServerAddress);
					break;
			}
			return converted.ToString();
		}
	}
}
