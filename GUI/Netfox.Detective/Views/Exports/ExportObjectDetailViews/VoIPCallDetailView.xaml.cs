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

using Netfox.Core.Interfaces.Views.Exports;

namespace Netfox.Detective.Views.Exports.ExportObjectDetailViews
{
    /// <summary>
    ///     Interaction logic for VoIPCallDetailView.xaml
    /// </summary>
    public partial class VoIPCallDetailView : DetectiveExportDetailPaneViewBase, IVoIPCallDetailView
    {
        public VoIPCallDetailView()
        {
            this.InitializeComponent();
            //InitializeComponent();

            //CallsList.ItemContainerGenerator.StatusChanged += (sender, e) =>
            //{
            //    CallsList.Dispatcher.Invoke(() =>
            //    {


            //        if (CallsList.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //        {

            //            if (engines != null)
            //            {
            //                foreach (var nAudioEngine in engines)
            //                {
            //                    nAudioEngine.Value.Dispose();
            //                }
            //            }

            //            engines.Clear();

            //            //engines.Clear();
            //            foreach (var item in CallsList.Items)
            //            {
            //                var it = CallsList.ItemContainerGenerator.ContainerFromItem(item);
            //                var listBoxItem = it as RadListBoxItem;


            //                var timeLine = FindFirstElementInVisualTree<WaveformTimeline>(listBoxItem);

            //                if (timeLine != null)
            //                {

            //                    var callContent = item as VoIpExport.VoIPCall.CallContent;

            //                    if (callContent.AbsoluteWavPath != null && File.Exists(callContent.AbsoluteWavPath))
            //                    {
            //                        var sEngine = new NAudioEngine();
            //                        timeLine.RegisterSoundPlayer(sEngine);
            //                        sEngine.OpenFile(callContent.AbsoluteWavPath);
            //                        engines.Add(
            //                            new KeyValuePair<VoIpExport.VoIPCall.CallContent, NAudioEngine>(callContent,
            //                                sEngine));

            //                    }
            //                    else
            //                        timeLine.IsEnabled = false;
            //                }
            //            }
            //        }
            //        // Use The One
            //    });
            //};
        }
    }
}