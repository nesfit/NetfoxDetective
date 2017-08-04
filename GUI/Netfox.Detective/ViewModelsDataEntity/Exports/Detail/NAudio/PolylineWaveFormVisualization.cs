using NAudio.Dsp;

namespace Netfox.GUI.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    class PolylineWaveFormVisualization : IVisualizationPlugin
    {
        private PolylineWaveFormControl polylineWaveFormControl = new PolylineWaveFormControl();

        public string Name => "Polyline WaveForm Visualization";

        public object Content => this.polylineWaveFormControl;

        public void OnMaxCalculated(float min, float max)
        {
            this.polylineWaveFormControl.AddValue(max, min);
        }

        public void OnFftCalculated(Complex[] result)
        {
            // nothing to do
        }


    }
}