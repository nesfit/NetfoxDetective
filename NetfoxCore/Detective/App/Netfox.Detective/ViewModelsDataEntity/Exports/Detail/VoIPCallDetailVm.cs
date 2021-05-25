using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail.NAudio;
using Netfox.Framework.Models.Snoopers;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    [NotifyPropertyChanged]
    public class VoIPCallDetailVm : DetectiveExportDetailPaneViewModelBase
    {
        private RelayCommand _pauseCommand;

        //[IgnoreAutoChangeNotification]
        //public RelayCommand OpenFileCommand => new RelayCommand(this.OpenFile);
        private RelayCommand _playCommand;
        private RelayCommand _stopCommand;

        public VoIPCallDetailVm(WindsorContainer applicationWindsorContainer, ExportVm model, IVoIPCallDetailView view)
            : base(applicationWindsorContainer, model, view)
        {
            try
            {
                this.IsHidden = !this.ExportVm.Calls?.Any() ?? true;
                this.IsActive = this.ExportVm.Calls?.Any() ?? false;
                this.DockPositionPosition = DetectiveDockPosition.DockedDocument;
                this.ExportVmObserver.RegisterHandler(p => p.SelectedSnooperExportObject, p =>
                {
                    if (p.SelectedSnooperExportObject is ICall)
                    {
                        try
                        {
                            this.SelectedCall = p.SelectedSnooperExportObject as ICall;
                            this.IsHidden = false;
                            this.IsActive = true;

                            this.UpdateCallStreams();

                            this.CallStreams = this.SelectedCall.CallStreams
                                .Select(callStream => new CallStreamVm(callStream)).ToArray();
                            this.PossibleCallStreams = this.SelectedCall.PossibleCallStreams
                                .Select(callStream => new CallStreamVm(callStream)).ToArray();

                            this.CPlay.RaiseCanExecuteChanged();
                            this.CPause.RaiseCanExecuteChanged();
                            this.CStop.RaiseCanExecuteChanged();
                        }
                        catch (Exception ex)
                        {
                            this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
                            this.SelectedCall = null;
                            this.IsHidden = true;
                            this.IsActive = false;
                        }
                    }
                    else
                    {
                        this.SelectedCall = null;
                        this.IsHidden = true;
                        this.IsActive = false;
                    }
                });
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"{this.GetType().Name} instantiation failed", ex);
            }
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "VoIP call detail";

        #endregion

        public ICall SelectedCall { get; private set; }
        public IEnumerable<CallStreamVm> CallStreams { get; private set; }
        public IEnumerable<CallStreamVm> PossibleCallStreams { get; private set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CPlay => this._playCommand ?? (this._playCommand = new RelayCommand(() =>
        {
            foreach (var call in this.CallStreams)
            {
                call.Play();
            }

            foreach (var call in this.PossibleCallStreams)
            {
                call.Play();
            }

            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }, () =>
        {
            var canCallStreams = this.CallStreams?.Any(call => call.NAudioEngine.CanPlay);
            var canPossibleCallStreams = this.PossibleCallStreams?.Any(call => call.NAudioEngine.CanPlay);
            if (canCallStreams.HasValue && canCallStreams.Value)
            {
                return true;
            }

            if (canPossibleCallStreams.HasValue && canPossibleCallStreams.Value)
            {
                return true;
            }

            return false;
        }));

        [IgnoreAutoChangeNotification]
        public RelayCommand CPause => this._pauseCommand ?? (this._pauseCommand = new RelayCommand(() =>
        {
            foreach (var call in this.CallStreams)
            {
                call.Pause();
            }

            foreach (var call in this.PossibleCallStreams)
            {
                call.Pause();
            }

            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }, () =>
        {
            var canCallStreams = this.CallStreams?.Any(call => call.NAudioEngine.CanPause);
            var canPossibleCallStreams = this.PossibleCallStreams?.Any(call => call.NAudioEngine.CanPause);
            if (canCallStreams.HasValue && canCallStreams.Value)
            {
                return true;
            }

            if (canPossibleCallStreams.HasValue && canPossibleCallStreams.Value)
            {
                return true;
            }

            return false;
        }));

        [IgnoreAutoChangeNotification]
        public RelayCommand CStop => this._stopCommand ?? (this._stopCommand = new RelayCommand(() =>
        {
            foreach (var call in this.CallStreams)
            {
                call.Stop();
            }

            foreach (var call in this.PossibleCallStreams)
            {
                call.Stop();
            }

            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }, () =>
        {
            var canCallStreams = this.CallStreams?.Any(call => call.NAudioEngine.CanStop);
            var canPossibleCallStreams = this.PossibleCallStreams?.Any(call => call.NAudioEngine.CanStop);
            if (canCallStreams.HasValue && canCallStreams.Value)
            {
                return true;
            }

            if (canPossibleCallStreams.HasValue && canPossibleCallStreams.Value)
            {
                return true;
            }

            return false;
        }));

        private void UpdateCallStreams()
        {
            var callStreams =
                this.ApplicationOrInvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>()
                    .SelectMany(export => export.ExportObjects.Where(eobj => eobj is ICallStream))
                    .Where(
                        stream =>
                            this.SelectedCall.CallStreamAddresses.Contains(stream.SourceEndPoint) &&
                            this.SelectedCall.CallStreamAddresses.Contains(stream.DestinationEndPoint));
            foreach (var callStream in callStreams)
            {
                if (this.SelectedCall.CallStreams.Contains(callStream as ICallStream))
                {
                    return;
                }

                this.SelectedCall.CallStreams.Add(callStream as ICallStream);
            }


            var possibleCallStreams =
                this.ApplicationOrInvestigationWindsorContainer
                    .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>()
                    .SelectMany(export => export.ExportObjects.Where(eobj => eobj is ICallStream))
                    .Where(
                        stream =>
                            this.SelectedCall.CallStreamAddresses.Contains(stream.SourceEndPoint) ||
                            this.SelectedCall.CallStreamAddresses.Contains(stream.DestinationEndPoint))
                    .Where(callStream => !this.SelectedCall.CallStreams.Contains(callStream as ICallStream));

            foreach (var possibleCallStream in possibleCallStreams)
            {
                if (this.SelectedCall.PossibleCallStreams.Contains(possibleCallStream as ICallStream))
                {
                    return;
                }

                this.SelectedCall.PossibleCallStreams.Add(possibleCallStream as ICallStream);
            }
        }
    }
}