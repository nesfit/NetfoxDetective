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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.AnalyzerVoIP.Interfaces;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Core;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using Netfox.SnooperRTP.Models;
using Netfox.SnooperSIP.Models;
using PostSharp.Patterns.Model;

namespace Netfox.AnalyzerVoIP.ViewModels
{
    public class VoIPOverviewVm : DetectiveIvestigationDataEntityPaneViewModelBase, IAnalyzer
    {
        public VoIPOverviewVm(WindsorContainer investigationOrAppWindsorContainer, IConversationsVm model) : base(investigationOrAppWindsorContainer, model)
        {
            this.ConversationsVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IVoIPOverviewView>());

            this.SipExports = this.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportSIP>>(new
            {
                investigationWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
            this.RtpExports = this.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportRTP>>(new
            {
                investigationWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
        }
        public VoIPOverviewVm(WindsorContainer investigationOrAppWindsorContainer) : base(investigationOrAppWindsorContainer, null)
        {
            this.ConversationsVm = null;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IVoIPOverviewView>());

            this.SipExports = this.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportSIP>>(new
            {
                investigationWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
            this.RtpExports = this.ApplicationOrInvestigationWindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportRTP>>(new
            {
                investigationWindsorContainer = this.ApplicationOrInvestigationWindsorContainer
            });
        }
        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                var conversationsVm = this.ConversationsVm;
                if(conversationsVm != null) { conversationsVm.IsSelected = value; }
            }
        }

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "VoIP overview";
        #endregion

        //public ObservableCollection<VoIPStatItem> VoIPSIPRTPAssignedDistribution { get; } = new ObservableCollection<VoIPStatItem>();
        //public ObservableCollection<VoIPStatItem> VoIPRTPsAssignedToSipVsAll { get; } = new ObservableCollection<VoIPStatItem>();
        [SafeForDependencyAnalysis]
        public BindablePeriodicalTask<IEnumerable<VoIPStatItem>> VoIPSIPRTPAssignedDistributionTasked
            => new BindablePeriodicalTask<IEnumerable<VoIPStatItem>>(this.UpdateVoIPSIPRTPAssignedDistribution, new[]
            {
                this.RtpExports,
                this.SipExports as INotifyCollectionChanged
            });
        [SafeForDependencyAnalysis]
        public BindablePeriodicalTask<IEnumerable<VoIPStatItem>> VoIPRTPsAssignedToSipVsAllTasked
            => new BindablePeriodicalTask<IEnumerable<VoIPStatItem>>(this.UpdateVoIPRTPsAssignedToSipVsAll, new[]
            {
                this.RtpExports,
                this.SipExports as INotifyCollectionChanged
            });

        public IConversationsVm ConversationsVm { get; }

        public VirtualizingObservableDBSetPagedCollection<SnooperExportRTP> RtpExports { get; }

        private VirtualizingObservableDBSetPagedCollection<SnooperExportSIP> SipExports { get; }

        public async Task<IEnumerable<VoIPStatItem>> UpdateVoIPRTPsAssignedToSipVsAll()
        {
            return await Task.Run((() =>
            {
                var allSipCalls = this.SipExports.SelectMany(se => se.ExportObjects.OfType<SIPCall>());
                var sipCalls = allSipCalls as SIPCall[] ?? allSipCalls.ToArray();

                var allRtPsAssignedToSip = sipCalls.SelectMany(sc => sc.CallStreams).Concat(sipCalls.SelectMany(sc => sc.PossibleCallStreams)).Distinct();
                var allRtps = this.RtpExports.SelectMany(rtp => rtp.ExportObjects);

                return new List<VoIPStatItem>
                {
                    new VoIPStatItem(VoIPStatItem.VoIPType.AllRtPsAssignedToSip, allRtPsAssignedToSip.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.AllRtps, allRtps.Count())
                };
            }));
        }

        public async Task<IEnumerable<VoIPStatItem>> UpdateVoIPSIPRTPAssignedDistribution()
        {
            return await Task.Run((() =>
            {
                var allSipCalls = this.SipExports.SelectMany(se => se.ExportObjects.OfType<SIPCall>());
                var sipCalls = allSipCalls as SIPCall[] ?? allSipCalls.ToArray();
                var sipWithoutRTPStream = sipCalls.Where(c => !c.PossibleCallStreams.Any() && !c.CallStreams.Any());
                var sipWithOnlyPossibleRTPStreams = sipCalls.Where(c => c.PossibleCallStreams.Any() && !c.CallStreams.Any());
                var sipWithOneRTPStreamAndNonPossible = sipCalls.Where(c => c.CallStreams.Count == 1 && !c.PossibleCallStreams.Any());
                var sipWithOneRTPStreamAndAnyPossible = sipCalls.Where(c => c.CallStreams.Count == 1 && c.PossibleCallStreams.Any());
                var sipWithTwoRTPStreams = sipCalls.Where(c => c.CallStreams.Count == 2);
                var sipWithTwoAndMoreRTPStreams = sipCalls.Where(c => c.CallStreams.Count > 2);

                return new List<VoIPStatItem>
                {
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithoutRTPStream, sipWithoutRTPStream.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithOnlyPossibleRTPStreams, sipWithOnlyPossibleRTPStreams.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithOneRTPStreamAndNonPossible, sipWithOneRTPStreamAndNonPossible.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithOneRTPStreamAndAnyPossible, sipWithOneRTPStreamAndAnyPossible.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithTwoRTPStreams, sipWithTwoRTPStreams.Count()),
                    new VoIPStatItem(VoIPStatItem.VoIPType.SipWithTwoAndMoreRTPStreams, sipWithTwoAndMoreRTPStreams.Count())
                };
            }));
        }

        public class VoIPStatItem
        {
            public enum VoIPType
            {
                SipWithoutRTPStream,
                SipWithOnlyPossibleRTPStreams,
                SipWithOneRTPStreamAndNonPossible,
                SipWithOneRTPStreamAndAnyPossible,
                SipWithTwoRTPStreams,
                SipWithTwoAndMoreRTPStreams,

                AllRtPsAssignedToSip,
                AllRtps
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public VoIPStatItem(VoIPType voIPItemType, int value)
            {
                this.VoIPItemType = voIPItemType;
                this.Value = value;
            }

            public VoIPType VoIPItemType { get; }

            public int Value { get; }
        }

    }
}