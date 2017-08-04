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
using Netfox.AppIdent.Accord;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Statistics;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

//using Catharsis.Commons;
namespace Netfox.AppIdent.Tests
{
        [TestFixture]
        public class ApplicationRecognizerAppIDentTest : FrameworkBaseTests
        {

            public bool OmmitClassifiedModelDetails => this.L7Conversations.Count() > 1000;

            public void ProcessPcapFile(string pcapFilePath) { this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath)); }

            public ApplicationProtocolClassificationStatisticsMeter EpiTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                out EPIEvaluator epiEvaluator,
                int minFlows = 1)
            {
                this.ProcessPcapFile(pcapFilePath);
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations,minFlows, trainingToClassifyingRatio);
               return this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector(), out  epiEvaluator);
            }

            public ApplicationProtocolClassificationStatisticsMeter MLTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                double precisionTrashHold = 0.99,
                int minFlows = 1)
            {
                this.ProcessPcapFile(pcapFilePath);
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, minFlows, trainingToClassifyingRatio);
                return  this.AppIdentService.BayesianClassify(appIdentDataSource,trainingToClassifyingRatio,precisionTrashHold);
               
            }
        
            [SetUp]
            public void SetUp()
            {
                base.SetUpInMemory();
            }

            public AppIdentService AppIdentService { get; set; } = new AppIdentService();

            [Test][Explicit][Category("Explicit")]
            public void CompareStatisticsTest()
            {
                this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);
                var precMeasure = this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector());
                precMeasure.PrintResults();
                this.CompareStatistics(precMeasure, "testMeasure.xml");

            }

            [Test][Explicit][Category("Explicit")]
            public void EPIselfTest()
            {
                this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));


                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);

                var epiEvaluator = new EPIEvaluator(new FeatureSelector());
                epiEvaluator.CreateApplicationProtocolModels(appIdentDataSource.TrainingSet);
                var precMeasure = epiEvaluator.ComputeStatistics(appIdentDataSource.TrainingSet);

                var consoleDefaultColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleDefaultColor;
                Console.WriteLine("################# Procotol model details: ####################");
                epiEvaluator.PrintProtocolModels();

                Console.WriteLine("################# Procotol similarities: ####################");
                epiEvaluator.AgregateProtocolModels();

                Console.WriteLine("################# Summary: ####################");
                precMeasure.PrintResults();
                //applicationProtocolModelEvaluator.PrintCsvProtocolModels();

                this.AppIdentService.SaveStatisticsToxml("testMeasure.xml", precMeasure);
            }

          
            protected EPIEvaluator EpiTestCliBase(string pcapFilePath, double trainingToClassifyingRatio)
            {
                var precMeasure = this.EpiTestBase(pcapFilePath, trainingToClassifyingRatio, out var epiEvaluator);

                Console.WriteLine("################# Procotol model details: ####################");
                epiEvaluator.PrintProtocolModels();

                Console.WriteLine("################# Procotol similarities: ####################");
                epiEvaluator.AgregateProtocolModels();

                Console.WriteLine("################# Summary: ####################");
                precMeasure.PrintResults();

                //this.SaveModelForMeasure(applicationProtocolModelEvaluator, precMeasure);
                //applicationProtocolModelEvaluator.PrintCsvProtocolModels();

                Console.WriteLine("################# Application Protocol Models Hierachival Clustering: ####################");

                epiEvaluator.PrintClusters();
                return epiEvaluator;
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItest30()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.3);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItest70()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.7);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItest90()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItest90M2()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }

   
            public EPIEvaluator MlEpiTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                out ApplicationProtocolClassificationStatisticsMeter epiprecMeasure,
                out ApplicationProtocolClassificationStatisticsMeter mlprecMeasure,
                double precisionTrashHold,
                int minFlows = 1)
            {
                   this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations,minFlows,trainingToClassifyingRatio);
                EPIEvaluator epiEvaluator;
                epiprecMeasure = this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector(), out epiEvaluator);
                mlprecMeasure = this.AppIdentService.BayesianClassify(appIdentDataSource,0.7,precisionTrashHold);
                return epiEvaluator;
            }




            protected void MLTestCliBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold = 0.99)
            {
                var precMeasure = this.MLTestBase(pcapFilePath, trainingToClassifyingRatio, precisionTrashHold);
                precMeasure.PrintResults();
            }


            [Test][Explicit][Category("Explicit")]
            public void BayesNetworkTest_M2()
            {
                Console.WriteLine("\n##### Bayes Network with 30% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.3);
                Console.WriteLine("\n##### Bayes Network with 70% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.7);
                Console.WriteLine("\n##### Bayes Network with 90% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }




            [Test][Explicit][Category("Explicit")]
            public void MLtest30()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.3);
            }


            [Test][Explicit][Category("Explicit")]
            public void MLtest70()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.7);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtest90()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_appIdent_test()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_appIdent_test()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_appIdent_test1()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test1.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_appIdent_test1()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test1.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_appIdent_test2()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test2.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_appIdent_test2()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test2.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_appIdent1()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent1.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_appIdent1()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent1.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_pcr_20160415()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160415.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_pcr_20160415()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160415.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void EPItestipluskal_pcr_20160416()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160416.cap", 0.9);
            }

            [Test][Explicit][Category("Explicit")]
            public void MLtestipluskal_pcr_20160416()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160416.cap", 0.9);
            }

            private void CompareStatistics(ApplicationProtocolClassificationStatisticsMeter newStatistics, string fileNameOldStatsXml)
            {
                var oldStatistics = this.AppIdentService.LoadStatisticsFromXml(fileNameOldStatsXml);


                int newScore = 0;
                int oldScore = 0;
                bool change = false;

                Console.WriteLine("##############Compare statistics##############");
                foreach(var applicationStatistic in newStatistics.AppStatistics)
                {
                    if(!oldStatistics.AppStatistics.ContainsKey(applicationStatistic.Key))
                    {
                        Console.WriteLine("Missing application " + applicationStatistic.Key + " in old statistics.\n");
                        continue;
                    }

                    var oldStat = oldStatistics.AppStatistics[applicationStatistic.Key];
                    var newPrec = applicationStatistic.Value.Precission;
                    var oldPrec = oldStat.Precission;
                    if(newPrec.CompareTo(oldPrec) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newPrec.CompareTo(oldPrec) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    var newRec = applicationStatistic.Value.Recall;
                    var oldRec = oldStat.Recall;
                    if(newRec.CompareTo(oldRec) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newRec.CompareTo(oldRec) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    var newFmes = applicationStatistic.Value.FMeasure;
                    var oldFmes = oldStat.FMeasure;
                    if(newFmes.CompareTo(oldFmes) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newFmes.CompareTo(oldFmes) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    if(change)
                    {
                        Console.WriteLine(applicationStatistic.Key);
                        Console.WriteLine("New model TP: " + applicationStatistic.Value.TP + " FP: " + applicationStatistic.Value.FP + " FN: " + applicationStatistic.Value.FN);
                        Console.WriteLine("Old model TP: " + oldStat.TP + " FP: " + oldStat.FP + " FN: " + oldStat.FN);
                        Console.WriteLine("New model/Old model Precision: " + newPrec + "/" + oldPrec);
                        Console.WriteLine("New model/Old model Recall: " + newRec + "/" + oldRec);
                        Console.WriteLine("New model/Old model F-Measure: " + newFmes + "/" + oldFmes);
                        Console.WriteLine();
                    }
                    change = false;
                }

                Console.WriteLine("Score new/old: " + newScore + "/" + oldScore);
            }


        }
    }

