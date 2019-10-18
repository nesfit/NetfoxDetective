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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using GalaSoft.MvvmLight.CommandWpf;
using Netfox.AnalyzerAppIdent.Properties;
using Netfox.AnalyzerAppIdent.Services;
using Netfox.AppIdent;
using Netfox.AppIdent.Accord;
using Netfox.Core.Helpers;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Persistence;

namespace Netfox.AnalyzerAppIdent.ViewModels
{
    public class AppIdentControllerVm:INotifyPropertyChanged
    {
        public AppIdentMainVm AppIdentMainVm { get; set; }
        public ILogger Logger => this.AppIdentMainVm?.Logger;
        private double _trainingToClassifyingRatio = 0.7;
        private RelayCommandAsync _runClassificationCommand;
        private string _status = _allPcapWillBeUsed;
        private string _minimumFlowsPerTrainingLabel = "30";
        private RelayCommand _startPacketCapturingCommand;
        private double _precisionTrashHold = 0.99;
        private static string _allPcapWillBeUsed = "All PCAP will be used!";

        public double PrecisionTrashHold
        {
            get => this._precisionTrashHold;
            set
            {
                if(value.Equals(this._precisionTrashHold)) return;
                this._precisionTrashHold = value;
                this.OnPropertyChanged();
            }
        }

        public AppIdentControllerVm(AppIdentMainVm appIdentMainVm)
        {
            this.AppIdentMainVm = appIdentMainVm;
        }


        public double TrainingToClassifyingRatio
        {
            get => this._trainingToClassifyingRatio;
            set
            {
                if(Math.Abs(value - this._trainingToClassifyingRatio) < 0.001) { return; }
                this._trainingToClassifyingRatio = value;
                this.OnPropertyChanged();
            }
        }

        public RelayCommandAsync RunClassificationCommand => this._runClassificationCommand ??
            (this._runClassificationCommand = new RelayCommandAsync(async () =>
        {
            await this.RunClassification();
        }, this.CheckAnyCapture));

        private bool CheckAnyCapture()
        {
            var selectedCapture = this.GetSelectedCapture();
            this.Status = selectedCapture != null? $"Pcap> {selectedCapture.FileInfo.Name} will be used." : _allPcapWillBeUsed;
            return this.AppIdentMainVm.ApplicationShell.CurrentInvestigationVm.Captures.Any();
        }

        public string Status
        {
            get => this._status;
            set
            {
                if(value == this._status) { return; }
                this._status = value;
                this.OnPropertyChanged();
            }
        }

        public string MinimumFlowsPerTrainingLabel
        {
            get => this._minimumFlowsPerTrainingLabel;
            set
            {
                if(value == this._minimumFlowsPerTrainingLabel) { return; }
                this._minimumFlowsPerTrainingLabel = value;
                this.OnPropertyChanged();
            }
        }

        public RelayCommand StartPacketCapturingCommand
        {
            get
            {
                return this._startPacketCapturingCommand ?? (this._startPacketCapturingCommand = new RelayCommand(() =>
                {
                    var nmCapturer = new Capturer(@"d:\test123456.cap");
                    nmCapturer.Start();
                    Thread.Sleep(5000);
                    nmCapturer.Stop();
                    //nmCapturer.Test();
                }));
            }
        }

        [Netfox.Core.Attributes.AsyncTask(Title = nameof(RunClassification), Description = "Running AppIdent recognition.")]
        private async Task RunClassification()
        {

            this.Status = "Classifying...";
            int minFlows;
            if(!int.TryParse(this.MinimumFlowsPerTrainingLabel,out minFlows))
            {
                this.Status = "MinimumFlowsPerTrainingLabel has to be an integer... corrent and run again...";
                this.Logger?.Info(this.Status);
            }
            await Task.Run(() =>
            {
                var selectedCapture = this.GetSelectedCapture();

                using (var dbx = this.AppIdentMainVm.ApplicationOrInvestigationWindsorContainer.Resolve<NetfoxDbContext>())
                {
                    var selectedl7ConvsInNvestigation = selectedCapture == null? dbx.L7Conversations : dbx.L7Conversations.Where(l7 => l7.Captures.Any(c => c.Id == selectedCapture.Id));

                    var l7Conversations = selectedl7ConvsInNvestigation.Include(l7 => l7.Frames)
                        .Include(l7 => l7.L3Conversation)
                        .Include(l7 => l7.L4Conversation)
                        .Include(l7 => l7.UnorderedL7PDUs.Select(l7PDU => l7PDU.FrameList))
                        .Include(l7 => l7.UnorderedL7PDUs.Select(l7PDU => l7PDU.L7Conversation)).ToArray();

                    if(!l7Conversations.Any())
                    {
                        this.Status = "Classification failed, no l7 conversations...";
                        this.Logger?.Error(this.Status);
                        return;
                    }
                    var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(l7Conversations, minFlows, this.TrainingToClassifyingRatio);
                    if(!appIdentDataSource.TrainingSet.Any())
                    {
                        this.Status = $"No conversation contains at least {minFlows}";
                        this.Logger?.Error(this.Status);
                        return;
                    }
                    var epiClasify = this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector(), out var epiEvaluator);
                    this.AppIdentMainVm.AppProtoModelEval = epiEvaluator;
                    this.AppIdentMainVm.EpiPrecMeasure = epiClasify;
                    this.AppIdentMainVm.MlPrecMeasure = this.AppIdentService.BayesianClassify(appIdentDataSource,this.TrainingToClassifyingRatio,this.PrecisionTrashHold);
                }
                this.Status = "Classification done, investigate results...";
                this.Logger?.Info(this.Status);
            });
        }

        private PmCaptureBase GetSelectedCapture() { return this.AppIdentMainVm.ApplicationShell.CurrentInvestigationVm.SelectedCaptureVms.FirstOrDefault()?.Capture; }

        public AppIdentService AppIdentService { get; set; } = new AppIdentService();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
