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
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Analysis;
using Netfox.AppIdent.Accord;
using Netfox.AppIdent.Misc;
using Netfox.AppIdent.Statistics;
using Netfox.CorrelationMatrixChart;
using Netfox.FrameworkAPI.Tests;
using NUnit.Framework;
using Accord.Math;
using Netfox.FrameworkAPI.Tests;

//using Vector = numl.Math.LinearAlgebra.Vector;

namespace Netfox.AppIdent.Tests
{
    [TestFixture]
    public class AppIdentAccordTests : FrameworkBaseTests
    {
        public AppIdentAccordTests()
        {
            this.AccordAppIdent = new AccordAppIdent();
        }


        public AccordAppIdent AccordAppIdent { get; }

        public bool OmmitClassifiedModelDetails => this.L7Conversations.Count() > 1000;

        public AppIdentService AppIdentService { get; set; } = new AppIdentService();


        [Test]
        //[DotMemoryUnit(CollectAllocations = true)]
        //[AssertTraffic(AllocatedSizeInBytes = 1024*1024)]
        public void CorrelationMatrixTest_M2()
        {
            MainWindow window = null;
            this.ProcessPcapFile(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_dnsHttpTls_cap));
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            var featureSelection = new FeatureSelection();

            var selection = featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.3);
            var correlationMatrix = selection.Last();


            // The dispatcher thread
            var t = new Thread(() =>
            {
                window = new MainWindow
                {
                    DataContext = new MainViewModel(appIdentAcordSource.FeatureNames, correlationMatrix)
                };
                // Initiates the dispatcher thread shutdown when the window closes
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();
                // Makes the thread support message pumping
                Dispatcher.Run();
            });
            // Configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        [Test]
        public void DecisionTreeTest_M2()
        {
            Console.WriteLine("\n##### Decision Tree with 30% threshold #####\n");
            this.DecisionTreeTestCliBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_testM2_cap), 0.3);
            Console.WriteLine("\n##### Decision Tree with 70% threshold #####\n");
            this.DecisionTreeTestCliBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_testM2_cap), 0.7);
            Console.WriteLine("\n##### Decision Tree with 90% threshold #####\n");
            this.DecisionTreeTestCliBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_testM2_cap), 0.9);
        }



        public void ProcessPcapFile(string pcapFilePath) { this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath)); }

        [Test]
        [Explicit]
        [Category("Explicit")]
        public void RandomForestsCrossValidationTest_M2()
        {
            //TODO: 20210430 - Move files to PcapPath class
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_13\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_14\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_15\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_16\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_17\20-4\data0.cap");

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, 30);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            Console.WriteLine($"Feature vectors: {featureVectors.Length}");
            var featureSelection = new FeatureSelection();
            featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.5);

            foreach(var feature in appIdentAcordSource.FeatureSelector.SelectedFeatures)
            {
                Console.WriteLine($"{feature.Name}");
            }

            var rfcModel = this.AccordAppIdent.GetBestRandomForestsWithGridSearch(appIdentAcordSource, out var bestParameters, out var minError);
            // get the cross validation results
            var cvResults = this.AccordAppIdent.GetCrossValidationResultsOfRandomForestModel(appIdentAcordSource, bestParameters);
            Console.WriteLine("### CV Results ###");
            Console.WriteLine("\n### Training stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Training.Mean, 6), Math.Round(cvResults.Training.StandardDeviation, 6));
            Console.WriteLine("\n### Validation stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Validation.Mean, 6), Math.Round(cvResults.Validation.StandardDeviation, 6));

            var minErorr = cvResults.Validation.Values.Min();
            var bestIndex = cvResults.Validation.Values.IndexOf(minErorr);
            var classifier = cvResults.Models[bestIndex];

            var validationDataSource = classifier.Tag as AccordAppIdent.ValidationDataSource;
            var predictedValues = classifier.Model.Decide(validationDataSource.ValidationInputs);

            var i = 0;
            foreach (var label in appIdentAcordSource.Labels.Distinct())
            {
                var conf = new ConfusionMatrix(validationDataSource.ValidationOutputs, predictedValues, i++);
                Console.WriteLine($"##########################");
                Console.WriteLine($"Protocol: {label}");
                Console.WriteLine($"TP: {conf.TruePositives}");
                Console.WriteLine($"TN: {conf.TrueNegatives}");
                Console.WriteLine($"FN: {conf.FalseNegatives}");
                Console.WriteLine($"FP: {conf.FalsePositives}");
                Console.WriteLine($"Accuracy: {conf.Accuracy}");
                Console.WriteLine($"Precision: {conf.Precision}");
                Console.WriteLine($"Specificity: {conf.Specificity}");
                Console.WriteLine($"Sensitivity: {conf.Sensitivity}");
                Console.WriteLine($"FScore: {conf.FScore}");
            }
        }

        [Test]
        public void RandomForestsCrossValidationTest_2()
        {
            this.ProcessPcapFile(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_dnsHttpTls_cap));

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, 10);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            Console.WriteLine($"Feature vectors: {featureVectors.Length}");
            var featureSelection = new FeatureSelection();
            featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.5);

            foreach (var feature in appIdentAcordSource.FeatureSelector.SelectedFeatures)
            {
                Console.WriteLine($"{feature.Name}");
            }

            this.AccordAppIdent.GetBestRandomForestsWithGridSearch(appIdentAcordSource, out var bestParameters, out var minError);
            // get the cross validation results
            var cvResults = this.AccordAppIdent.GetCrossValidationResultsOfRandomForestModel(appIdentAcordSource, bestParameters);
            Console.WriteLine("### CV Results ###");
            Console.WriteLine("\n### Training stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Training.Mean, 6), Math.Round(cvResults.Training.StandardDeviation, 6));
            Console.WriteLine("\n### Validation stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Validation.Mean, 6), Math.Round(cvResults.Validation.StandardDeviation, 6));

            var minErorr = cvResults.Validation.Values.Min();
            var bestIndex = cvResults.Validation.Values.IndexOf(minErorr);
            var classifier = cvResults.Models[bestIndex];

            var labels = appIdentAcordSource.Labels.Distinct();

            var modelStore = new AppIdentTestContext();
            var model = classifier.Model;
            var randomForestFilePath =  modelStore.Save(model, labels);
            modelStore.Load<RandomForest>(randomForestFilePath, out var randomForestModel, out var labelsLoaded);


            var validationDataSource = classifier.Tag as AccordAppIdent.ValidationDataSource;
            var predictedValues = randomForestModel.Decide(validationDataSource.ValidationInputs);

            var i = 0;
            foreach (var label in labelsLoaded)
            {
                var conf = new ConfusionMatrix(validationDataSource.ValidationOutputs, predictedValues, i++);
                Console.WriteLine($"##########################");
                Console.WriteLine($"Protocol: {label}");
                Console.WriteLine($"TP: {conf.TruePositives}");
                Console.WriteLine($"TN: {conf.TrueNegatives}");
                Console.WriteLine($"FN: {conf.FalseNegatives}");
                Console.WriteLine($"FP: {conf.FalsePositives}");
                Console.WriteLine($"Accuracy: {conf.Accuracy}");
                Console.WriteLine($"Precision: {conf.Precision}");
                Console.WriteLine($"Specificity: {conf.Specificity}");
                Console.WriteLine($"Sensitivity: {conf.Sensitivity}");
                Console.WriteLine($"FScore: {conf.FScore}");
            }
        }



        [SetUp]
        public void SetUp()
        {
            this.SetUpInMemory();
        }

        protected void DecisionTreeTestCliBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold = 0.99)
        {
            var precMeasure = this.DecisionTreeTestBase(pcapFilePath, trainingToClassifyingRatio, precisionTrashHold);
            precMeasure.PrintResults();
        }


        // DECISION TREE CLF

        private ApplicationProtocolClassificationStatisticsMeter DecisionTreeTestBase(
            string pcapFilePath,
            double trainingToClassifyingRatio,
            double precisionTrashHold = 0.99,
            int minFlows = 1)
        {
            this.ProcessPcapFile(pcapFilePath);
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, minFlows, trainingToClassifyingRatio);
            return this.AppIdentService.DecisionTreeClassify(appIdentDataSource, trainingToClassifyingRatio, precisionTrashHold);
        }
    }
}