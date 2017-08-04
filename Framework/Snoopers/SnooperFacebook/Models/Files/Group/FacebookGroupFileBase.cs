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
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperFacebook.Models.Base;

namespace Netfox.SnooperFacebook.Models.Files.Group
{
	public abstract class FacebookGroupFileBase : FacebookBase
	{
        protected FacebookGroupFileBase() : base() { } //EF
        /// <summary>
        /// Base class for sending files between three or more users.
        /// </summary>
        /// <param name="exportBase"></param>
		protected FacebookGroupFileBase(SnooperExportBase exportBase) : base(exportBase)
		{
			this.ParticipantsId = new List<ulong>();
		}
		public List<ulong> ParticipantsId { get; set; }
		public string Url { get; set; }
        public string Name { get; set; }
		public string GroupName { get; set; }
	}
}
