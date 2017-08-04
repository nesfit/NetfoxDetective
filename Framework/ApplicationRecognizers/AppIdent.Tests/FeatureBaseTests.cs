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

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Netfox.AppIdent.Features.Bases;
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using Netfox.NetfoxFrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.AppIdent.Tests
{
    public abstract class FeatureBaseTests<TFeature> : FrameworkBaseTests where TFeature : FeatureBase
    {
        protected L7Conversation L7ConversationTesting { get; set; }
        protected L7Conversation L7ConversationTraining1 { get; set; }
        protected L7Conversation L7ConversationTraining2 { get; set; }

        [Test]
        public void ComputeDistanceToProtocolModel_SelfDistance_Zero()
        {
            var feature = this.ComputeFeature(this.L7ConversationTesting, DaRFlowDirection.up);
            var distance = feature.ComputeDistanceToProtocolModel(feature);
            AssertDistanceValue(feature, distance, 0);
        }

        public abstract void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_ExpectedDistance();

        public void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_NotZero(double expectedDistance)
        {
            const DaRFlowDirection direction = DaRFlowDirection.up;
            var featureTraining1 = this.ComputeFeature(this.L7ConversationTraining1, direction);
            var featureTraining2 = this.ComputeFeature(this.L7ConversationTraining2, direction);
            var modelFeature = this.CreateFeature(direction);
            var featureCollection = new FeatureCollectionWrapperMock<TFeature>(new[]
            {
                featureTraining1,
                featureTraining2
            });
            modelFeature.InitializeNormalization(featureCollection);
            modelFeature.ComputeFeatureForProtocolModel(featureCollection);

            var featureTesting = this.ComputeFeature(this.L7ConversationTesting, direction);

            var distance = modelFeature.ComputeDistanceToProtocolModel(featureTesting);
            AssertDistanceValue(modelFeature, distance, expectedDistance);
        }

        public TFeature ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection daRFlowDirection, double expectedValue)
        {
            var feature = this.ComputeFeature(this.L7ConversationTesting, daRFlowDirection);
            AssertComputedFeatureMLValue(feature, expectedValue);
            return feature;
        }

        public abstract void ComputeFeature_FeatureValueDownFlow_ExpectedFeatureValue();

        public abstract void ComputeFeature_FeatureValueNonFlow_ExpectedFeatureValue();

        public abstract void ComputeFeature_FeatureValueUpFlow_ExpectedFeatureValue();

        protected static void AssertComputedFeatureMLValue(FeatureBase feature, double expectedValue)
        {
            Assert.AreEqual(expectedValue, feature.FeatureValue, $"{feature.GetType().Name} - ComputeFeature returned incorrect value in {feature.FlowDirection} direction.");
        }

        protected static void AssertDistanceValue(FeatureBase feature, double distance, double expectedValue)
        {
            Assert.AreEqual(expectedValue, distance, $"{feature.GetType().Name} - ComputeDistanceToProtocolModel returned incorrect value in {feature.FlowDirection} direction.");
        }

        protected TFeature ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection direction)
        {
            return Activator.CreateInstance(typeof(TFeature), l7Conversation, direction) as TFeature;
        }

        protected FeatureBase CreateFeature(DaRFlowDirection direction)
        {
            var feature = Activator.CreateInstance(typeof(TFeature)) as FeatureBase;
            Debug.Assert(feature != null, "feature != null");
            feature.FlowDirection = direction;
            return feature;
        }

        protected L7Conversation GetL7ConversationForIp(IPEndPoint ipEndPoint) { return this.L7Conversations.First(c => c.SourceEndPoint.Equals(ipEndPoint)); }

        protected virtual void OneTimeSetUp(string pcapFilePath)
        {
            this.SetUpInMemory();
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath));
        }
        [SetUp]
        public void SetUp() { base.SingleTestWaitOne(); }
    }
}