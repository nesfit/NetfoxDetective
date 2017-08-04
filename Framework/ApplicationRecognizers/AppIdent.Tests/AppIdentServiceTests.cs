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
using Netfox.AppIdent.Accord;
using Netfox.AppIdent.Misc;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.AppIdent.Tests
{
    [TestFixture]
    public class AppIdentServiceTests : FrameworkBaseTests
    {


        [SetUp]
        public void SetUp()
        {
            this.SetUpInMemory();
        }
   

        public AppIdentService AppIdentService { get; set; } = new AppIdentService();

        public void ProcessPcapFile(string pcapFilePath)
        {
            Console.WriteLine($"{DateTime.Now}, adding capture: {pcapFilePath}");
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath));
        }
        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void CreateApplicationProtocolModels_particioning_1()
        {
            var nowDateTime = DateTime.Now;
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.CreateApplicationProtocolModels_particioning_1), nowDateTime)
            {
                MinFlows = 10,
                TrainingToVerificationRation = 1,
            };
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext.TrainingToVerificationRation);

            this.AppIdentTestContext.Save(appIdentDataSource);
            var actual = appIdentDataSource.TrainingSet.Length + (appIdentDataSource?.VerificationSet?.Length ?? 0);
            Assert.AreEqual(57, actual);


            var context = new AppIdentTestContext(nameof(this.RandomForestsCrossValidationTest_2), nowDateTime)
            {
                MinFlows = 10,
                TrainingToVerificationRation = 1,
            };
            var source = context.LoadAppIdentDataSource();
            Assert.AreEqual(57, source.TrainingSet.Length + (source?.VerificationSet?.Length ?? 0));
        }
        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void CreateApplicationProtocolModels_particioning_CreateDataSource()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.CreateApplicationProtocolModels_particioning_CreateDataSource))
            {
                MinFlows = 30,
                TrainingToVerificationRation = 1,
            };
            var pcapSource = new AppIdentPcapSource();
            pcapSource.AddTesting(@"D:\pcaps\AppIdent-TestingData\captured\", "*.cap|*.pcap", true);
            this.AppIdentTestContext.Save(pcapSource);

            foreach (var pcap in pcapSource.TestingPcaps) { this.ProcessPcapFile(pcap); }
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext.TrainingToVerificationRation);
            this.AppIdentTestContext.Save(appIdentDataSource);
        }

      

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void RandomForestsCrossValidationTest_2()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.RandomForestsCrossValidationTest_2))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                CrossValidationFolds = 10,
                TrainingToVerificationRation = 1,
            };
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows,this.AppIdentTestContext.TrainingToVerificationRation);
            var featureSelector = this.AppIdentService.EliminateCorelatedFeatures(appIdentDataSource, this.AppIdentTestContext.FeatureSelectionTreshold, this.AppIdentTestContext);
            var bestParameters = this.AppIdentService.RandomForestGetBestParameters(appIdentDataSource, featureSelector, this.AppIdentTestContext);
            var classificationStatisticsMeter = this.AppIdentService.RandomForestCrossValidation(appIdentDataSource, this.AppIdentTestContext.FeatureSelector, bestParameters, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save();
        }


        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_fast()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_fast))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                CrossValidationFolds = 10,
            };
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);

            var featureSelector = this.AppIdentService.EliminateCorelatedFeatures(appIdentDataSource, this.AppIdentTestContext.FeatureSelectionTreshold, this.AppIdentTestContext);

            var bestParameters = this.AppIdentService.RandomForestGetBestParameters(appIdentDataSource,featureSelector, this.AppIdentTestContext);

            this.L7Conversations.Clear();
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);
            

            appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);

            var  classificationStatisticsMeter = this.AppIdentService.RandomForestCrossValidation(appIdentDataSource, this.AppIdentTestContext.FeatureSelector, bestParameters, this.AppIdentTestContext.CrossValidationFolds, this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save();
        }

        public AppIdentTestContext AppIdentTestContext { get; set; }

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_appIdent1()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_appIdent1))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                CrossValidationFolds = 10,
            };
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_13\20-4\data0.cap");

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);
            var featureSelector = this.AppIdentService.EliminateCorelatedFeatures(appIdentDataSource, this.AppIdentTestContext.FeatureSelectionTreshold, this.AppIdentTestContext);
            var bestParameters = this.AppIdentService.RandomForestGetBestParameters(appIdentDataSource, featureSelector, this.AppIdentTestContext);

            this.L7Conversations.Clear();

            
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_14\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_15\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_16\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_17\20-4\data0.cap");


            appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);
            this.AppIdentTestContext.Save(appIdentDataSource);
            var classificationStatisticsMeter = this.AppIdentService.RandomForestCrossValidation(appIdentDataSource, this.AppIdentTestContext.FeatureSelector, bestParameters, this.AppIdentTestContext.CrossValidationFolds, this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save();
        }

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_ICDF()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.RandomForestsCrossValidationTest_LearnCompletelyDifferentPcap_ICDF))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                CrossValidationFolds = 2
            };

            var pcapSource = new AppIdentPcapSource();
            pcapSource.AddTesting(@"D:\pcaps\AppIdent-TestingData\captured\pc_13\20-4\data0.cap");

            pcapSource.AddVerification(@"D:\pcaps\AppIdent-TestingData\captured\","*.cap|*.pcap",true);
            this.AppIdentTestContext.Save(pcapSource);

            foreach (var pcap in pcapSource.TestingPcaps){this.ProcessPcapFile(pcap);}

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);
            var featureSelector = this.AppIdentService.EliminateCorelatedFeatures(appIdentDataSource, this.AppIdentTestContext.FeatureSelectionTreshold, this.AppIdentTestContext);
            var bestParameters = this.AppIdentService.RandomForestGetBestParameters(appIdentDataSource, featureSelector, this.AppIdentTestContext);

            this.L7Conversations.Clear();

            foreach (var pcap in pcapSource.VerificationPcaps)
            {
                this.ProcessPcapFile(pcap);
            }
            appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows);

            var classificationStatisticsMeter = this.AppIdentService.RandomForestCrossValidation(appIdentDataSource, featureSelector, bestParameters, this.AppIdentTestContext.CrossValidationFolds, this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save();
        }

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void EPI_FullFeatureSet_fast()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.EPI_FullFeatureSet_fast))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                TrainingToVerificationRation = 0.7
            };
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            
            var featureSelector = new FeatureSelector();

            this.L7Conversations.Clear();
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);


            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext.TrainingToVerificationRation);

            var classificationStatisticsMeter = this.AppIdentService.EpiClasify(appIdentDataSource, featureSelector,  this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save(classificationStatisticsMeter);
            this.AppIdentTestContext.Save();
        }

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void EPI_FullFeatureSet_ICDF()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.EPI_FullFeatureSet_ICDF))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                TrainingToVerificationRation = 0.7
            };

            var pcapSource = new AppIdentPcapSource();
            var featureSelector = new FeatureSelector();

            pcapSource.AddTesting(@"D:\pcaps\AppIdent-TestingData\captured\", "*.cap|*.pcap", true);
            this.AppIdentTestContext.Save(pcapSource);

            foreach (var pcap in pcapSource.TestingPcaps) { this.ProcessPcapFile(pcap); }

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows,this.AppIdentTestContext.TrainingToVerificationRation);

            var classificationStatisticsMeter = this.AppIdentService.EpiClasify(appIdentDataSource, featureSelector, this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save(classificationStatisticsMeter);
            this.AppIdentTestContext.Save();
        }

        [Test][Explicit][NUnit.Framework.Category("Explicit")]
        public void EPI_FeatureSelection_fast()
        {
            this.AppIdentTestContext = new AppIdentTestContext(nameof(this.EPI_FeatureSelection_fast))
            {
                MinFlows = 10,
                FeatureSelectionTreshold = 0.5,
                TrainingToVerificationRation = 0.7
            };


            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext.TrainingToVerificationRation);
            var featureSelector = this.AppIdentService.EliminateCorelatedFeatures(appIdentDataSource, this.AppIdentTestContext.FeatureSelectionTreshold, this.AppIdentTestContext);

            this.L7Conversations.Clear();

            var pcapSource = new AppIdentPcapSource();
            pcapSource.AddTesting(@"D:\pcaps\AppIdent-TestingData\captured\", "*.cap|*.pcap", true);
            this.AppIdentTestContext.Save(pcapSource);

            foreach (var pcap in pcapSource.TestingPcaps) { this.ProcessPcapFile(pcap); }

            appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, this.AppIdentTestContext.MinFlows, this.AppIdentTestContext.TrainingToVerificationRation);

            var classificationStatisticsMeter = this.AppIdentService.EpiClasify(appIdentDataSource, featureSelector,  this.AppIdentTestContext);
            classificationStatisticsMeter.PrintResults();
            this.AppIdentTestContext.Save();
        }
    }
}