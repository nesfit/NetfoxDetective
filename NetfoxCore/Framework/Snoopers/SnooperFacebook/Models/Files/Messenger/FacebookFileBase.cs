﻿// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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

using Netfox.Framework.Models.Snoopers;
using SnooperFacebook.Models.Base;

namespace SnooperFacebook.Models.Files.Messenger
{
    /// <summary>
    /// Base class for sending files between two users.
    /// </summary>
	public abstract class FacebookFileBase : FacebookBase
	{
        protected FacebookFileBase() : base() { } //EF
        protected FacebookFileBase(SnooperExportBase exportBase) : base(exportBase)
		{
		}
		public ulong TargetId { get; set; }
		public string Url { get; set; }
        public string Name { get; set; }
	}
}
