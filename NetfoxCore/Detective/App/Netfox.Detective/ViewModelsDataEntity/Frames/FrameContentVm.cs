using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Framework.Models.PmLib.Frames;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Frames
{
    public class FrameContentVm : DetectiveIvestigationDataEntityPaneViewModelBase, IFrameContentView
    {
        public FrameContentVm(WindsorContainer applicationWindsorContainer, FrameVm model) : base(
            applicationWindsorContainer, model)
        {
            this.FrameVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IFrameContentView>());
        }

        #region Overrides of DetectivePaneViewModelBase

        [SafeForDependencyAnalysis]
        public override string HeaderText => "Frame content: " + this.FrameVm.Frame.SourceEndPoint + " - " +
                                             this.FrameVm.Frame.DestinationEndPoint;

        #endregion

        public FrameVm FrameVm { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CNextFrame => new RelayCommand(() =>
        {
            if (this.FrameVm == null)
            {
                return;
            }

            this.FrameVm.ApplicationOrInvestigationWindsorContainer.Release(this);
            var frameModel =
                this.FrameVm.ApplicationOrInvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>()
                    .FirstOrDefault(f =>
                        f.FrameIndex == this.FrameVm.FrameIndex + 1 && f.PmCapture == this.FrameVm.FwFrame.PmCapture);
            if (frameModel == null)
            {
                return;
            }

            this.FrameVm = this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new Dictionary<string, object>
            {
                {"model", frameModel},
                {"investigationOrAppWindsorContainer", this.ApplicationOrInvestigationWindsorContainer}
            });
        });

        [IgnoreAutoChangeNotification]
        public RelayCommand CPreviousFrame => new RelayCommand(() =>
        {
            if (this.FrameVm == null)
            {
                return;
            }

            this.FrameVm.ApplicationOrInvestigationWindsorContainer.Release(this);
            var frameModel =
                this.FrameVm.ApplicationOrInvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>()
                    .FirstOrDefault(f =>
                        f.FrameIndex == this.FrameVm.FrameIndex - 1 && f.PmCapture == this.FrameVm.FwFrame.PmCapture);
            if (frameModel == null)
            {
                return;
            }

            this.FrameVm = this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new Dictionary<string, object>
            {
                {"model", frameModel},
                {"investigationOrAppWindsorContainer", this.ApplicationOrInvestigationWindsorContainer}
            });
        });

        [IgnoreAutoChangeNotification]
        public RelayCommand<Guid> CChangeFrame => new RelayCommand<Guid>((frameId) =>
        {
            if (this.FrameVm == null)
            {
                return;
            }

            this.FrameVm.ApplicationOrInvestigationWindsorContainer.Release(this);
            var frameModel =
                this.FrameVm.ApplicationOrInvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>()
                    .FirstOrDefault(f => f.Id == frameId && f.PmCapture == this.FrameVm.FwFrame.PmCapture);
            if (frameModel == null)
            {
                return;
            }

            this.FrameVm = this.FrameVm.ApplicationOrInvestigationWindsorContainer.Resolve<FrameVm>(new Dictionary<string, object>
            {
                {"model", frameModel},
                {"investigationOrAppWindsorContainer", this.ApplicationOrInvestigationWindsorContainer}
            });
        });
    }
}