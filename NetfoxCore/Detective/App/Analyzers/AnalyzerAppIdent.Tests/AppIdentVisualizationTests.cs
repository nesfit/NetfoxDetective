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

using System.Threading;
using Netfox.AnalyzerAppIdent.ViewModels;
using Netfox.AnalyzerAppIdent.Views;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Statistics;
using Netfox.AppIdent.Tests;
using Netfox.FrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.AnalyzerAppIdent.Tests
{
    [TestFixture]
    public class AppIdentVisualizationTests : ApplicationRecognizerAppIDentTest
    {
        [Test]
        public void EPItestMerged()
        {
            // this.EpiVisualizationBase(@"D:\pcaps\pcpluskal2\merge.cap", 0.9);
            this.EpiVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.appIdent_large_merge), 0.9);
        }
        [Test]
        public void EPItest30_1() {
            this.EpiVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_dnsHttpTls_cap), 0.3);
        }
        [Test]
        public void MLtest30_1()
        {
            this.MLVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_dnsHttpTls_cap), 0.3);
        }
        [Test]
        public void EPItestipluskal_appIdent_test_1()
        {
            // this.EpiVisualizationBase(@"D:\pcaps\pcpluskal2\appIdent_test.cap", 0.9);
            this.EpiVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.appIdent_test), 0.9);
        }

        [Test]
        public void EpiMLtest30_1()
        {
            // this.EpiMLVisualizationBase(@"D:\pcaps\pcpluskal2\appIdent_test1.cap", 0.3, 0.99);
            this.EpiMLVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.appIdent_test1), 0.3, 0.99);
        }

        [Test][Explicit]
        public void EpiMltestipluskal_appIdent_test1_1()
        {
            this.EpiMLVisualizationBase(PcapPath.GetPcap(PcapPath.Pcaps.app_identification_dnsHttpTls_cap), 0.3, 0.99);
        }

        private void EpiMLVisualizationBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold)
        {
            ApplicationProtocolClassificationStatisticsMeter epiprecMeasure;
            ApplicationProtocolClassificationStatisticsMeter mlprecMeasure;
            var appProtoModelEval = this.MlEpiTestBase(pcapFilePath, trainingToClassifyingRatio, out epiprecMeasure, out mlprecMeasure,precisionTrashHold);
            ShowResultsInApp(appProtoModelEval, epiprecMeasure, mlprecMeasure);
        }
        

        private void EpiVisualizationBase(string pcapFilePath, double trainingToClassifyingRatio)
        {
            var epiprecMeasure = this.EpiTestBase(pcapFilePath, trainingToClassifyingRatio, out var epiEvaluator,1);
            epiprecMeasure.PrintResults();
            ShowResultsInApp(epiEvaluator, epiprecMeasure,null);
        }
        private void MLVisualizationBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold = 0.99)
        {
            var mlprecMeasure = this.MLTestBase(pcapFilePath, trainingToClassifyingRatio,precisionTrashHold, 1);
            ShowResultsInApp(null,null, mlprecMeasure);
        }

        private static void ShowResultsInApp(EPIEvaluator appProtoModelEval, ApplicationProtocolClassificationStatisticsMeter epiprecMeasure, ApplicationProtocolClassificationStatisticsMeter mlprecMeasure)
        {
            WrapperWindow mainView = null;
            var t = new Thread(() =>
            {
                mainView = new WrapperWindow
                {
                    DataContext = new AppIdentMainVm(appProtoModelEval, epiprecMeasure, mlprecMeasure)
                };
                // Initiates the dispatcher thread shutdown when the mainView closes
                mainView.Closed += (s, e) => mainView.Dispatcher.InvokeShutdown();

                mainView.Show();

                // Makes the thread support message pumping
                System.Windows.Threading.Dispatcher.Run();
            });

            // Configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }


   

}
