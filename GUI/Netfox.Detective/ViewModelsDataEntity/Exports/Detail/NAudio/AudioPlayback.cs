using System;
using System.Windows;
using NAudio.Wave;

namespace Netfox.GUI.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    class AudioPlayback : IDisposable
    {
        private IWavePlayer playbackDevice;
        private WaveStream fileStream;

        public event EventHandler<FftEventArgs> FftCalculated;

        protected virtual void OnFftCalculated(FftEventArgs e)
        {
            var handler = this.FftCalculated;
            handler?.Invoke(this, e);
        }

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        protected virtual void OnMaximumCalculated(MaxSampleEventArgs e)
        {
            var handler = this.MaximumCalculated;
            handler?.Invoke(this, e);
        }

        public void Load(string fileName)
        {
            this.Stop();
            this.CloseFile();
            this.EnsureDeviceCreated();
            this.OpenFile(fileName);
        }

        private void CloseFile()
        {
            if(this.fileStream == null) { return; }
            this.fileStream.Dispose();
            this.fileStream = null;
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new AudioFileReader(fileName);
                this.fileStream = inputStream;
                var aggregator = new SampleAggregator(inputStream)
                {
                    NotificationCount = inputStream.WaveFormat.SampleRate/100,
                    PerformFFT = true
                };
                aggregator.FftCalculated += (s, a) => this.OnFftCalculated(a);
                aggregator.MaximumCalculated += (s, a) => this.OnMaximumCalculated(a);
                this.playbackDevice.Init(aggregator);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                this.CloseFile();
            }
        }

        private void EnsureDeviceCreated()
        {
            if (this.playbackDevice == null)
            {
                this.CreateDevice();
            }
        }

        private void CreateDevice()
        {
            this.playbackDevice = new WaveOut { DesiredLatency = 200 };
        }

        public void Play()
        {
            if (this.playbackDevice != null && this.fileStream != null && this.playbackDevice.PlaybackState != PlaybackState.Playing)
            {
                this.playbackDevice.Play();
            }
        }

        public void Pause()
        {
            this.playbackDevice?.Pause();
        }

        public void Stop()
        {
            this.playbackDevice?.Stop();
            if (this.fileStream != null)
            {
                this.fileStream.Position = 0;
            }
        }

        public void Dispose()
        {
            this.Stop();
            this.CloseFile();
            if (this.playbackDevice != null)
            {
                this.playbackDevice.Dispose();
                this.playbackDevice = null;
            }
        }
    }
}
