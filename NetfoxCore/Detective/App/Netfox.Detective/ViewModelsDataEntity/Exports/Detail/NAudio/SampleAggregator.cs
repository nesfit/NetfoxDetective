using System;
using NAudio.Dsp;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    public class SampleAggregator
    {
        private int binaryExponentitation;
        private int bufferSize;
        private Complex[] channelData;
        private int channelDataPosition;

        public SampleAggregator(int bufferSize)
        {
            this.bufferSize = bufferSize;
            this.binaryExponentitation = (int) Math.Log(bufferSize, 2);
            this.channelData = new Complex[bufferSize];
        }

        public float LeftMaxVolume { get; private set; }

        public float LeftMinVolume { get; private set; }

        public float RightMaxVolume { get; private set; }

        public float RightMinVolume { get; private set; }

        /// <summary>
        ///     Add a sample value to the aggregator.
        /// </summary>
        /// <param name="value">The value of the sample.</param>
        public void Add(float leftValue, float rightValue)
        {
            if (this.channelDataPosition == 0)
            {
                this.LeftMaxVolume = float.MinValue;
                this.RightMaxVolume = float.MinValue;
                this.LeftMinVolume = float.MaxValue;
                this.RightMinVolume = float.MaxValue;
            }

            // Make stored channel data stereo by averaging left and right values.
            this.channelData[this.channelDataPosition].X = (leftValue + rightValue) / 2.0f;
            this.channelData[this.channelDataPosition].Y = 0;
            this.channelDataPosition++;

            this.LeftMaxVolume = Math.Max(this.LeftMaxVolume, leftValue);
            this.LeftMinVolume = Math.Min(this.LeftMinVolume, leftValue);
            this.RightMaxVolume = Math.Max(this.RightMaxVolume, rightValue);
            this.RightMinVolume = Math.Min(this.RightMinVolume, rightValue);

            if (this.channelDataPosition >= this.channelData.Length)
            {
                this.channelDataPosition = 0;
            }
        }

        public void Clear()
        {
            this.LeftMaxVolume = float.MinValue;
            this.RightMaxVolume = float.MinValue;
            this.LeftMinVolume = float.MaxValue;
            this.RightMinVolume = float.MaxValue;
            this.channelDataPosition = 0;
        }

        /// <summary>
        ///     Performs an FFT calculation on the channel data upon request.
        /// </summary>
        /// <param name="fftBuffer">A buffer where the FFT data will be stored.</param>
        public void GetFFTResults(float[] fftBuffer)
        {
            var channelDataClone = new Complex[this.bufferSize];
            this.channelData.CopyTo(channelDataClone, 0);
            FastFourierTransform.FFT(true, this.binaryExponentitation, channelDataClone);
            for (var i = 0; i < channelDataClone.Length / 2; i++)
            {
                // Calculate actual intensities for the FFT results.
                fftBuffer[i] = (float) Math.Sqrt(channelDataClone[i].X * channelDataClone[i].X +
                                                 channelDataClone[i].Y * channelDataClone[i].Y);
            }
        }
    }
}