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

/*
 *
 *
 */

using System;
using System.Collections.Generic;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Services;
using Netfox.NBARDatabase;

namespace Netfox.AppIdent
{
    //Todo IMPLEMENT
    public class ApplicationRecognizerAppIDent : ApplicationRecognizerBase
    {
        public ApplicationRecognizerAppIDent(NBARProtocolPortDatabase NBARProtocolPortDatabase) : base(NBARProtocolPortDatabase) { }

        public override String Name => @"AppIdent";

        public override String Description => @"Application identification using ML.";

        public override UInt32 Priority => 4;

        public override String Type => "ML, flow stats.";

        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation)
        {
            //var appTag = this.ModelExtractor.RunRecognition(conversation);
            //if (appTag == null) { return new List<NBAR2TaxonomyProtocol>(); }
            //return new List<NBAR2TaxonomyProtocol>
            //{
            //    this.NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(appTag)
            //};
            return null;
        }
    }
}