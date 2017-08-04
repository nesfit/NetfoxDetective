using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Netfox.GUI.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    /// <summary>
    /// Interaction logic for PolylineWaveFormControl.xaml
    /// </summary>
    public partial class PolylineWaveFormControl : UserControl, IWaveFormRenderer
    {
        int renderPosition;
        double yTranslate = 40;
        double yScale = 40;
        int blankZone = 10;

        Polyline topLine = new Polyline();
        Polyline bottomLine = new Polyline();

        public PolylineWaveFormControl()
        {
            this.SizeChanged += this.OnSizeChanged;
            this.InitializeComponent();
            this.topLine.Stroke = this.Foreground;
            this.bottomLine.Stroke = this.Foreground;
            this.topLine.StrokeThickness = 1;
            this.bottomLine.StrokeThickness = 1;
            this.mainCanvas.Children.Add(this.topLine);
            this.mainCanvas.Children.Add(this.bottomLine);
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            this.renderPosition = 0;
            this.ClearAllPoints();

            this.yTranslate = this.ActualHeight / 2;
            this.yScale = this.ActualHeight / 2;
        }

        private void ClearAllPoints()
        {
            this.topLine.Points.Clear();
            this.bottomLine.Points.Clear();
        }

        public void AddValue(float maxValue, float minValue)
        {
            var pixelWidth = (int) this.ActualWidth;
            if (pixelWidth > 0)
            {
                this.CreatePoint(maxValue, minValue);

                if (this.renderPosition > this.ActualWidth)
                {
                    this.renderPosition = 0;
                }
                var erasePosition = (this.renderPosition + this.blankZone) % pixelWidth;
                if (erasePosition < this.topLine.Points.Count)
                {
                    var yPos = this.SampleToYPosition(0);
                    this.topLine.Points[erasePosition] = new Point(erasePosition, yPos);
                    this.bottomLine.Points[erasePosition] = new Point(erasePosition, yPos);
                }
            }
        }

        private double SampleToYPosition(float value)
        {
            return this.yTranslate + value *this.yScale;
        }

        private void CreatePoint(float topValue, float bottomValue)
        {
            var topLinePos = this.SampleToYPosition(topValue);
            var bottomLinePos = this.SampleToYPosition(bottomValue);
            if (this.renderPosition >= this.topLine.Points.Count)
            {
                this.topLine.Points.Add(new Point(this.renderPosition, topLinePos));
                this.bottomLine.Points.Add(new Point(this.renderPosition, bottomLinePos));
            }
            else
            {
                this.topLine.Points[this.renderPosition] = new Point(this.renderPosition, topLinePos);
                this.bottomLine.Points[this.renderPosition] = new Point(this.renderPosition, bottomLinePos);
            }
            this.renderPosition++;
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
        public void Reset()
        {
            this.renderPosition = 0;
            this.ClearAllPoints();
        }
    }
}
