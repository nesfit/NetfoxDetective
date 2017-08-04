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

using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.SnooperFacebook.Models.Text
{
    /// <summary>
    /// Model for Facebook messenger message betweeen two users.
    /// </summary>
	public class FacebookMessage : FacebookTextBase, IChatMessage
    {
        private FacebookMessage() : base() { } //EF
        public FacebookMessage(SnooperExportBase exportBase) : base(exportBase) {}
		public ulong TargetId { get; set; }

        #region Implementation of IChatMessage
        public string Message => this.Text;
        public string Sender => this.SenderId.ToString();
        public string Receiver => this.TargetId.ToString();
        #endregion
    }
}
