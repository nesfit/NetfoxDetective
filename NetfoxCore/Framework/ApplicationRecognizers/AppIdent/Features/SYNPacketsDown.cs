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

using Netfox.AppIdent.Features.Bases;
using Netfox.Core.Enums;
using Netfox.Framework.Models;

namespace Netfox.AppIdent.Features
{
    public class SYNPacketsDown : SYNPacketsBase
    {
        public SYNPacketsDown() { }
        public SYNPacketsDown(L7Conversation l7Conversation) : base(l7Conversation, DaRFlowDirection.down) { }
        public SYNPacketsDown(double featureValue) : base(featureValue) { }
    }
}