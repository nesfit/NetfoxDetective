using System;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Interfaces.Model.Exports;
using PostSharp.Patterns.Model;
using WPFSoundVisualizationLib;
using WPFSoundVisualizationLib.WaveForm_Timeline;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    public class CallStreamVm
    {
        private RelayCommand _pauseCommand;

        //[IgnoreAutoChangeNotification]
        //public RelayCommand OpenFileCommand => new RelayCommand(this.OpenFile);
        private RelayCommand _playCommand;
        private RelayCommand _stopCommand;

        public CallStreamVm(ICallStream callStream)
        {
            this.CallStream = callStream;
            this.NAudioEngine.OpenFile(this.CallStream.WavFilePath);
            var timelineControl = new WaveformTimeline();
            timelineControl.RegisterSoundPlayer(this.NAudioEngine);
            this.Visualization = timelineControl;
        }

        public ICallStream CallStream { get; set; }
        public object Visualization { get; }
        public NAudioEngine NAudioEngine { get; } = new NAudioEngine();
        public bool IsPlayChecked { get; set; } = true;

        [IgnoreAutoChangeNotification]
        public RelayCommand CPlay => this._playCommand ??
                                       (this._playCommand = new RelayCommand(this.Play, () => this.NAudioEngine.CanPlay));

        [IgnoreAutoChangeNotification]
        public RelayCommand CPause => this._pauseCommand ??
                                        (this._pauseCommand =
                                            new RelayCommand(this.Pause, () => this.NAudioEngine.CanPause));

        [IgnoreAutoChangeNotification]
        public RelayCommand CStop => this._stopCommand ??
                                       (this._stopCommand = new RelayCommand(this.Stop, () => this.NAudioEngine.CanStop));

        public void Pause()
        {
            try
            {
                this.NAudioEngine.Pause();
            }
            catch (Exception)
            {
            }

            ;
            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }

        public void Play()
        {
            if (!this.IsPlayChecked)
            {
                return;
            }

            try
            {
                this.NAudioEngine.Play();
            }
            catch (Exception)
            {
            }

            ;

            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }

        public void Stop()
        {
            try
            {
                this.NAudioEngine.Stop();
            }
            catch (Exception)
            {
            }

            ;
            this.CPlay.RaiseCanExecuteChanged();
            this.CPause.RaiseCanExecuteChanged();
            this.CStop.RaiseCanExecuteChanged();
        }
    }
}