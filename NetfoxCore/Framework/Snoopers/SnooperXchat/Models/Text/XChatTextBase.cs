// Copyright (c) 2017 Jan Pluskal, Dudek Jindrich
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

namespace Netfox.Snoopers.SnooperXchat.Models.Text
{
    /// <summary>
    /// Base class for XChat private messages and group messages
    /// </summary>
    public abstract class XChatTextBase : SnooperExportedObjectBase
    {
        protected XChatTextBase() : base() { } //EF
        protected XChatTextBase(SnooperExportBase exportBase) : base(exportBase) {}
        public string Time { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
    }
}