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

using Netfox.Snoopers.SnooperSPDY.Models.Frames;

namespace Netfox.Snoopers.SnooperSPDY.Models
{
    public class SPDYResponseHeader : SPDYHeaderBase
    {
        public string StatusCode { get; private set; }

        public SPDYResponseHeader() { } //EF

        public SPDYResponseHeader(SPDYFrameSynReply synReplyFrame) : base(synReplyFrame as SPDYStreamFrame)
        {
            this.StatusCode = synReplyFrame.Fields[":status"];
        }

        public override string ToString() { return this.StatusCode; }
    }
}