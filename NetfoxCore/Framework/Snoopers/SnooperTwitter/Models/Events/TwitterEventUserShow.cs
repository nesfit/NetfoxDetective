// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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
using Netfox.Snoopers.SnooperSPDY.Models;

namespace Netfox.Snoopers.SnooperTwitter.Models.Events
{
    public class TwitterEventUserShow : TwitterEventBase
    {
        public string SourceUserId { get; private set; }
        public string TargetUserId { get; private set; }

        private TwitterEventUserShow() { }
        public TwitterEventUserShow(SnooperExportBase exportBase, SPDYMsg spdyMsg) : base(exportBase, spdyMsg)
        {
            var uriParams = (spdyMsg.Header as SPDYRequestHeader).URIParameters;

            this.SourceUserId = uriParams["source_id"];
            this.TargetUserId = uriParams["target_id"];
        }
    }
}