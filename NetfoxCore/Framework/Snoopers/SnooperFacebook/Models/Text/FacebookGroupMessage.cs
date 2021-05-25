// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace SnooperFacebook.Models.Text
{
    /// <summary>
    /// Model for Facebook messenger message betweeen three or more users.
    /// </summary>
	public class FacebookGroupMessage : FacebookTextBase, IChatGroupMessage
    {
        private FacebookGroupMessage() : base() { } //EF
        public FacebookGroupMessage(SnooperExportBase exportBase) : base(exportBase)
		{
			this.ParticipantsId = new List<ulong>();
		}
		public List<ulong> ParticipantsId { get; set; }
		public string GroupName { get; set; }

        #region Implementation of IChatMessage
        string IChatMessage.Message => this.Text;

        string IChatMessage.Sender => this.SenderId.ToString();

        string IChatMessage.Receiver
        {
            get
            {
                var sb = new StringBuilder();
                var delim = ", ";
                foreach(var id in this.ParticipantsId) { sb.Append(id);
                    sb.Append(delim);
                }
                sb.Remove(sb.Length - delim.Length-1, delim.Length);
                return sb.ToString();
            }
        }
        #endregion

        #region Implementation of IChatGroupMessage
        IEnumerable<string> IChatGroupMessage.Receivers => this.ParticipantsId.Select(id => id.ToString());
        #endregion
    }
}
