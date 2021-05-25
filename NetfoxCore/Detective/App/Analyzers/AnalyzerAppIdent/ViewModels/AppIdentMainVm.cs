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

using System.Reactive.Subjects;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.AnalyzerAppIdent.Interfaces;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Statistics;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.ViewModels;

namespace Netfox.AnalyzerAppIdent.ViewModels
{
    public class AppIdentMainVm : DetectiveInvestigationPaneViewModelBase, IAnalyzerInvestigation, IAnalyzerApplication
    {
        public override string HeaderText { get; set; } = "AppIdent";

        public EPIEvaluator AppProtoModelEval
        {
            get => this._appProtoModelEval;
            set { this._appProtoModelEval = value; this.InitVmsForNewCapture(); this.OnPropertyChanged();}
        }

        public ApplicationProtocolClassificationStatisticsMeter EpiPrecMeasure
        {
            get => this._epiPrecMeasure;
            set
            {
                if(Equals(value, this._epiPrecMeasure)) { return; }
                this._epiPrecMeasure = value;
                this.EpiPrecMeasureObservable.OnNext(value);
                this.OnPropertyChanged();
            }
        }

        public ApplicationProtocolClassificationStatisticsMeter MlPrecMeasure
        {
            get => this._mlPrecMeasure;
            set
            {
                this._mlPrecMeasure = value;
                this.MLAppIdentSummary = new AppIdentSummaryVm(this.MlPrecMeasure);
                this.OnPropertyChanged();
            }
        }

        public Subject<ApplicationProtocolClassificationStatisticsMeter> EpiPrecMeasureObservable { get; } = new Subject<ApplicationProtocolClassificationStatisticsMeter>();

        private ProtocolModelsVm _epiProtocolModelsVm;
        private AppIdentSummaryVm _epiAppIdentSummaryVm;
        private ProtocolModelsClusteringVm _epiProtocolModelsClusteringVm;
        private AppIdentControllerVm _appIdentControllerVm;
        private ApplicationProtocolClassificationStatisticsMeter _epiPrecMeasure;
        private EPIEvaluator _appProtoModelEval;
        private AppIdentSummaryVm _mlAppIdentSummary;
        private ApplicationProtocolClassificationStatisticsMeter _mlPrecMeasure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="appProtoModelEval"></param>
        /// <param name="epiPrecMeasure"></param>
        /// <param name="protoModelEval"></param>
        public AppIdentMainVm(WindsorContainer investigationOrAppWindsorContainer,EPIEvaluator appProtoModelEval, ApplicationProtocolClassificationStatisticsMeter epiPrecMeasure, ApplicationProtocolClassificationStatisticsMeter mlPrecMeasure) :this(investigationOrAppWindsorContainer)
        {
            this.AppProtoModelEval = appProtoModelEval;
            this.EpiPrecMeasure = epiPrecMeasure;
            this.MlPrecMeasure = mlPrecMeasure;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="appProtoModelEval"></param>
        /// <param name="epiPrecMeasure"></param>
        /// <param name="protoModelEval"></param>
        public AppIdentMainVm(EPIEvaluator appProtoModelEval, ApplicationProtocolClassificationStatisticsMeter epiPrecMeasure, ApplicationProtocolClassificationStatisticsMeter mlPrecMeasure) : this(null)
        {
            this.AppProtoModelEval = appProtoModelEval;
            this.EpiPrecMeasure = epiPrecMeasure;
            this.MlPrecMeasure = mlPrecMeasure;
        }

        private void InitVmsForNewCapture()
        {
            this.EpiAppIdentSummaryVm = new AppIdentSummaryVm(this);
            this.EpiProtocolModelsClusteringVm = new ProtocolModelsClusteringVm(this);
            this.EpiProtocolModelsVm = new ProtocolModelsVm(this);
        }

        public AppIdentMainVm(WindsorContainer investigationOrAppWindsorContainer):base(investigationOrAppWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IAppIdentMainView>());
            this.AppIdentControllerVm = new AppIdentControllerVm(this);
        }
        
        public AppIdentControllerVm AppIdentControllerVm
        {
            get => this._appIdentControllerVm;
            set
            {
                if(Equals(value, this._appIdentControllerVm)) { return; }
                this._appIdentControllerVm = value;
                this.OnPropertyChanged();
            }
        }

        public ProtocolModelsClusteringVm EpiProtocolModelsClusteringVm
        {
            get => this._epiProtocolModelsClusteringVm;
            set
            {
                if(Equals(value, this._epiProtocolModelsClusteringVm)) { return; }
                this._epiProtocolModelsClusteringVm = value;
                this.OnPropertyChanged();
            }
        }

        public AppIdentSummaryVm EpiAppIdentSummaryVm
        {
            get => this._epiAppIdentSummaryVm;
            set
            {
                if(Equals(value, this._epiAppIdentSummaryVm))
                {
                    return;
                }
                this._epiAppIdentSummaryVm = value;
                this.OnPropertyChanged();
            }
        }

        public ProtocolModelsVm EpiProtocolModelsVm
        {
            get => this._epiProtocolModelsVm;
            set
            {
                if(Equals(value, this._epiProtocolModelsVm)) { return; }
                this._epiProtocolModelsVm = value;
                this.OnPropertyChanged();
            }
        }
        
        public AppIdentSummaryVm MLAppIdentSummary
        {
            get => this._mlAppIdentSummary;
            set
            {
                if(Equals(value, this._mlAppIdentSummary)) { return; }
                this._mlAppIdentSummary = value;
                this.OnPropertyChanged();
            }
        }
    }
}
