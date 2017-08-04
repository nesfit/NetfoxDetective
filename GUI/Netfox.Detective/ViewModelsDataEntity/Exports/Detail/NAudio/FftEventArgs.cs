using System;
using System.Diagnostics;
using NAudio.Dsp;

namespace Netfox.GUI.Detective.ViewModelsDataEntity.Exports.Detail.NAudio
{
    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(Complex[] result)
        {
            this.Result = result;
        }
        public Complex[] Result { get; private set; }
    }
}
