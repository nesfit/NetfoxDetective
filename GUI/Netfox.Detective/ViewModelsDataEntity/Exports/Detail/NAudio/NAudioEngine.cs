// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using NAudio.Wave;
using WPFSoundVisualizationLib;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    public class NAudioEngine : INotifyPropertyChanged, ISpectrumPlayer, IWaveformPlayer, IDisposable
    {
        #region Constructor
        public NAudioEngine()
        {
            this.positionTimer.Interval = TimeSpan.FromMilliseconds(50);
            this.positionTimer.Tick += this.positionTimer_Tick;

            this.waveformGenerateWorker.DoWork += this.waveformGenerateWorker_DoWork;
            this.waveformGenerateWorker.RunWorkerCompleted += this.waveformGenerateWorker_RunWorkerCompleted;
            this.waveformGenerateWorker.WorkerSupportsCancellation = true;
        }
        #endregion

        #region Singleton Pattern
        public static NAudioEngine Instance
        {
            get
            {
                if(instance == null) { instance = new NAudioEngine(); }
                return instance;
            }
        }
        #endregion

        #region Private Utility Methods
        private void StopAndCloseStream()
        {
            if(this.waveOutDevice != null) { this.waveOutDevice.Stop(); }
            if(this.activeStream != null)
            {
                this.inputStream.Close();
                this.inputStream = null;
                this.ActiveStream.Close();
                this.ActiveStream = null;
            }
            if(this.waveOutDevice != null)
            {
                this.waveOutDevice.Dispose();
                this.waveOutDevice = null;
            }
        }
        #endregion

        #region Fields
        private static NAudioEngine instance;
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private readonly int fftDataSize = (int) FFTDataSize.FFT2048;
        private bool disposed;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool isPlaying;
        private bool inChannelTimerUpdate;
        private double channelLength;
        private double channelPosition;
        private bool inChannelSet;
        private WaveOut waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;
        private SampleAggregator sampleAggregator;
        private SampleAggregator waveformAggregator;
        private string pendingWaveformPath;
        private float[] fullLevelData;
        private float[] waveformData;
        private TimeSpan repeatStart;
        private TimeSpan repeatStop;
        private bool inRepeatSet;
        #endregion

        #region Constants
        private const int waveformCompressedPointCount = 2000;
        private const int repeatThreshold = 200;
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing) { this.StopAndCloseStream(); }

                this.disposed = true;
            }
        }
        #endregion

        #region ISpectrumPlayer
        public bool GetFFTData(float[] fftDataBuffer)
        {
            this.sampleAggregator.GetFFTResults(fftDataBuffer);
            return this.isPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            if(this.ActiveStream != null) {
                maxFrequency = this.ActiveStream.WaveFormat.SampleRate/2.0d;
            }
            else
            {
                maxFrequency = 22050; // Assume a default 44.1 kHz sample rate.
            }
            return (int) ((frequency/maxFrequency)*(this.fftDataSize/2));
        }
        #endregion

        #region IWaveformPlayer
        public TimeSpan SelectionBegin
        {
            get { return this.repeatStart; }
            set
            {
                if(!this.inRepeatSet)
                {
                    this.inRepeatSet = true;
                    var oldValue = this.repeatStart;
                    this.repeatStart = value;
                    if(oldValue != this.repeatStart) { this.NotifyPropertyChanged("SelectionBegin"); }
                    this.inRepeatSet = false;
                }
            }
        }

        public TimeSpan SelectionEnd
        {
            get { return this.repeatStop; }
            set
            {
                if(!this.inChannelSet)
                {
                    this.inRepeatSet = true;
                    var oldValue = this.repeatStop;
                    this.repeatStop = value;
                    if(oldValue != this.repeatStop) { this.NotifyPropertyChanged("SelectionEnd"); }
                    this.inRepeatSet = false;
                }
            }
        }

        public float[] WaveformData
        {
            get { return this.waveformData; }
            protected set
            {
                var oldValue = this.waveformData;
                this.waveformData = value;
                if(oldValue != this.waveformData) { this.NotifyPropertyChanged("WaveformData"); }
            }
        }

        public double ChannelLength
        {
            get { return this.channelLength; }
            protected set
            {
                var oldValue = this.channelLength;
                this.channelLength = value;
                if(oldValue != this.channelLength) { this.NotifyPropertyChanged("ChannelLength"); }
            }
        }

        public double ChannelPosition
        {
            get { return this.channelPosition; }
            set
            {
                if(!this.inChannelSet)
                {
                    this.inChannelSet = true; // Avoid recursion
                    var oldValue = this.channelPosition;
                    var position = Math.Max(0, Math.Min(value, this.ChannelLength));
                    if(!this.inChannelTimerUpdate && this.ActiveStream != null) {
                        this.ActiveStream.Position = (long) ((position/this.ActiveStream.TotalTime.TotalSeconds)*this.ActiveStream.Length);
                    }
                    this.channelPosition = position;
                    if(oldValue != this.channelPosition) { this.NotifyPropertyChanged("ChannelPosition"); }
                    this.inChannelSet = false;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) { if(this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(info)); } }
        #endregion

        #region Waveform Generation
        private class WaveformGenerationParams
        {
            public WaveformGenerationParams(int points, string path)
            {
                this.Points = points;
                this.Path = path;
            }

            public int Points { get; protected set; }
            public string Path { get; protected set; }
        }

        private void GenerateWaveformData(string path)
        {
            if(this.waveformGenerateWorker.IsBusy)
            {
                this.pendingWaveformPath = path;
                this.waveformGenerateWorker.CancelAsync();
                return;
            }

            if(!this.waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0) {
                this.waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, path));
            }
        }

        private void waveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled)
            {
                if(!this.waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0) {
                    this.waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, this.pendingWaveformPath));
                }
            }
        }

        private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var waveformParams = e.Argument as WaveformGenerationParams;
            //Mp3FileReader waveformMp3Stream;
            WaveFileReader waveformWaveStream;
            WaveChannel32 waveformInputStream;
            try
            {
                //waveformMp3Stream = new Mp3FileReader(waveformParams.Path);
                //waveformInputStream = new WaveChannel32(waveformMp3Stream);
                waveformWaveStream = new WaveFileReader(waveformParams.Path);
                waveformInputStream = new WaveChannel32(waveformWaveStream);
            }
            catch(Exception) {
                return; //why that object is set to null ref...?!
            }

            waveformInputStream.Sample += this.waveStream_Sample;

            var frameLength = this.fftDataSize;
            var frameCount = (int) (waveformInputStream.Length/(double) frameLength);
            var waveformLength = frameCount*2;
            var readBuffer = new byte[frameLength];
            this.waveformAggregator = new SampleAggregator(frameLength);

            var maxLeftPointLevel = float.MinValue;
            var maxRightPointLevel = float.MinValue;
            var currentPointIndex = 0;
            var waveformCompressedPoints = new float[waveformParams.Points];
            var waveformData = new List<float>();
            var waveMaxPointIndexes = new List<int>();

            for(var i = 1; i <= waveformParams.Points; i++) { waveMaxPointIndexes.Add((int) Math.Round(waveformLength*(i/(double) waveformParams.Points), 0)); }
            var readCount = 0;
            while(currentPointIndex*2 < waveformParams.Points)
            {
                waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

                waveformData.Add(this.waveformAggregator.LeftMaxVolume);
                waveformData.Add(this.waveformAggregator.RightMaxVolume);

                if(this.waveformAggregator.LeftMaxVolume > maxLeftPointLevel) { maxLeftPointLevel = this.waveformAggregator.LeftMaxVolume; }
                if(this.waveformAggregator.RightMaxVolume > maxRightPointLevel) { maxRightPointLevel = this.waveformAggregator.RightMaxVolume; }

                if(readCount > waveMaxPointIndexes[currentPointIndex])
                {
                    waveformCompressedPoints[(currentPointIndex*2)] = maxLeftPointLevel;
                    waveformCompressedPoints[(currentPointIndex*2) + 1] = maxRightPointLevel;
                    maxLeftPointLevel = float.MinValue;
                    maxRightPointLevel = float.MinValue;
                    currentPointIndex++;
                }
                if(readCount%3000 == 0)
                {
                    var clonedData = (float[]) waveformCompressedPoints.Clone();
                    DispatcherHelper.UIDispatcher.Invoke(() => { this.WaveformData = clonedData; });
                }

                if(this.waveformGenerateWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                readCount++;
            }

            var finalClonedData = (float[]) waveformCompressedPoints.Clone();
            DispatcherHelper.UIDispatcher.Invoke(() =>
            {
                this.fullLevelData = waveformData.ToArray();
                this.WaveformData = finalClonedData;
            });
            waveformInputStream.Close();
            waveformInputStream.Dispose();
            waveformInputStream = null;
            //waveformMp3Stream.Close();
            //waveformMp3Stream.Dispose();
            //waveformMp3Stream = null;
            waveformWaveStream.Close();
            waveformWaveStream.Dispose();
            waveformWaveStream = null;
        }
        #endregion

        #region Public Methods
        public void Stop()
        {
            if(this.waveOutDevice != null) { this.waveOutDevice.Stop(); }
            this.ChannelPosition = 0;
            this.IsPlaying = false;
            this.CanStop = false;
            this.CanPlay = true;
            this.CanPause = false;
        }

        public void Pause()
        {
            if(this.IsPlaying && this.CanPause)
            {
                this.waveOutDevice.Pause();
                this.IsPlaying = false;
                this.CanPlay = true;
                this.CanPause = false;
            }
        }

        public void Play()
        {
            if(this.CanPlay)
            {
                this.waveOutDevice.Play();
                this.IsPlaying = true;
                this.CanPause = true;
                this.CanPlay = false;
                this.CanStop = true;
            }
        }

        public void OpenFile(string path)
        {
            this.Stop();

            if(this.ActiveStream != null)
            {
                this.SelectionBegin = TimeSpan.Zero;
                this.SelectionEnd = TimeSpan.Zero;
                this.ChannelPosition = 0;
            }

            this.StopAndCloseStream();

            if(File.Exists(path))
            {
                try
                {
                    this.waveOutDevice = new WaveOut
                    {
                        DesiredLatency = 100
                    };
                    //  ActiveStream = new Mp3FileReader(path);
                    this.ActiveStream = new WaveFileReader(path);
                    this.inputStream = new WaveChannel32(this.ActiveStream);
                    this.sampleAggregator = new SampleAggregator(this.fftDataSize);
                    this.inputStream.Sample += this.inputStream_Sample;
                    this.waveOutDevice.Init(this.inputStream);
                    this.ChannelLength = this.inputStream.TotalTime.TotalSeconds;
                    this.GenerateWaveformData(path);
                    this.CanPlay = true;
                }
                catch
                {
                    this.ActiveStream = null;
                    this.CanPlay = false;
                }
            }
        }
        #endregion

        #region Public Properties
        public WaveStream ActiveStream
        {
            get { return this.activeStream; }
            protected set
            {
                var oldValue = this.activeStream;
                this.activeStream = value;
                if(oldValue != this.activeStream) { this.NotifyPropertyChanged("ActiveStream"); }
            }
        }

        public bool CanPlay
        {
            get { return this.canPlay; }
            protected set
            {
                var oldValue = this.canPlay;
                this.canPlay = value;
                if(oldValue != this.canPlay) { this.NotifyPropertyChanged("CanPlay"); }
            }
        }

        public bool CanPause
        {
            get { return this.canPause; }
            protected set
            {
                var oldValue = this.canPause;
                this.canPause = value;
                if(oldValue != this.canPause) { this.NotifyPropertyChanged("CanPause"); }
            }
        }

        public bool CanStop
        {
            get { return this.canStop; }
            protected set
            {
                var oldValue = this.canStop;
                this.canStop = value;
                if(oldValue != this.canStop) { this.NotifyPropertyChanged("CanStop"); }
            }
        }

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            protected set
            {
                var oldValue = this.isPlaying;
                this.isPlaying = value;
                if(oldValue != this.isPlaying) { this.NotifyPropertyChanged("IsPlaying"); }
                this.positionTimer.IsEnabled = value;
            }
        }
        #endregion

        #region Event Handlers
        private void inputStream_Sample(object sender, SampleEventArgs e)
        {
            this.sampleAggregator.Add(e.Left, e.Right);
            var repeatStartPosition = (long) ((this.SelectionBegin.TotalSeconds/this.ActiveStream.TotalTime.TotalSeconds)*this.ActiveStream.Length);
            var repeatStopPosition = (long) ((this.SelectionEnd.TotalSeconds/this.ActiveStream.TotalTime.TotalSeconds)*this.ActiveStream.Length);
            if(((this.SelectionEnd - this.SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && this.ActiveStream.Position >= repeatStopPosition)
            {
                this.sampleAggregator.Clear();
                this.ActiveStream.Position = repeatStartPosition;
            }
        }

        void waveStream_Sample(object sender, SampleEventArgs e) { this.waveformAggregator.Add(e.Left, e.Right); }

        void positionTimer_Tick(object sender, EventArgs e)
        {
            if(this.ActiveStream != null)
            {
                this.inChannelTimerUpdate = true;
                this.ChannelPosition = (this.ActiveStream.Position/(double) this.ActiveStream.Length)*this.ActiveStream.TotalTime.TotalSeconds;
                this.inChannelTimerUpdate = false;
            }
        }
        #endregion
    }
}