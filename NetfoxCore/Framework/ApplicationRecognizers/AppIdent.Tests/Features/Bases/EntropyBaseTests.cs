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

using System.Net;
using Netfox.AppIdent.Features.Bases;
using Netfox.Core.Enums;
using Netfox.FrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.AppIdent.Tests.Features.Bases
{
    [TestFixture]
    public class EntropyBaseTests : FeatureBaseTests<EntropyBase>
    {
        [Test]
        public override void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_ExpectedDistance()
        {
            this.ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_NotZero(0.36984415887793431d);
        }

        [Test]
        public override void ComputeFeature_FeatureValueDownFlow_ExpectedFeatureValue()
        {
            var feature = this.ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection.up, 4.7034564612986882d);
        }

        #region Overrides of FeatureBaseTests<EntropyBase>
        public override void ComputeFeature_FeatureValueNonFlow_ExpectedFeatureValue() { }
        #endregion

        [Test]
        public override void ComputeFeature_FeatureValueUpFlow_ExpectedFeatureValue()
        {
            var feature = this.ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection.down, 4.5329289824471397d);
          
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.OneTimeSetUp(PcapPath.GetPcap(PcapPath.Pcaps.features_three_conver_putty_ssh_cap));

            this.L7ConversationTesting = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21253));
            this.L7ConversationTraining1 = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21263));
            this.L7ConversationTraining2 = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21273));
        }

        #region Overrides of FeatureBaseTests<EntropyBase>
        #endregion
    }
}