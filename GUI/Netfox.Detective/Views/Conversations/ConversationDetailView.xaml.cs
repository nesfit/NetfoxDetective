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
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Messages.Exports;
using Netfox.Core.Messages.Views;
using Netfox.Detective.Models.Base;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;

namespace Netfox.Detective.Views.Conversations
{
    /// <summary>
    ///     Interaction logic for ConversationDetailView.xaml
    /// </summary>
    public partial class ConversationDetailView : DetectiveDataEntityPaneViewBase, IConversationDetailView
    {
        public static readonly DependencyProperty CDoubleClickedFrameProperty = DependencyProperty.Register(nameof(CDoubleClickedFrame), typeof(RelayCommand<FrameVm>),typeof(ConversationDetailView));

        public RelayCommand<FrameVm> CDoubleClickedFrame
        {
            get { return (RelayCommand<FrameVm>)this.GetValue(CDoubleClickedFrameProperty) ?? (this.DataContext as ConversationDetailVm)?.ConversationVm.CShowFrameDetail; }
            set { this.SetValue(CDoubleClickedFrameProperty, value); }
        }

        public FrameVm TrackedFrameVm
        {
            get { return this._trackedFrameVm; }
            private set
            {
                this._trackedFrameVm = value;
                var conversationDetailVm = this.ConversationDetailVm;
                if(conversationDetailVm != null) conversationDetailVm.ConversationVm.CurrentPacket = value;
            }
        }

        private bool _gotoFrameFlagTrack;
        private bool _gotoFrameFlagZoom;
        private FrameVm _trackedFrameVm;

        public ConversationDetailView()
        {
            this.InitializeComponent();
            this.DataContextChanged += (sender, args) => { this.ConversationDetailVm = args.NewValue as ConversationDetailVm; };
        }

        public ConversationDetailVm ConversationDetailVm { get; set; }

        private void ChartTrackBallBehavior_OnTrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            if(this._gotoFrameFlagTrack)
            {
                this._gotoFrameFlagTrack = false;
                return;
            }

            var closestDataPoint = e.Context.ClosestDataPoint;
            if(closestDataPoint == null) return;
            var data = closestDataPoint.DataPoint.DataItem as KeyValue<DateTime, long>;
            this.TrackedFrameVm = data?.Ref as FrameVm;
        }
        
        private void DownFlowQualityListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.NavigateToSelectedFrame();
        }

        private void ExportResultsListViewOnExportResultSelected(ExportVm exportResultVm, bool bringIntoView)
        {
            ExportResultMessage.SendExportResultMessage(exportResultVm, ExportResultMessage.MessageType.ExportResultSelected);
            if(bringIntoView) { BringToFrontMessage.SendBringToFrontMessage("ExportResultView"); }
        }

        private void FramesListViewOnFrameSelected(FrameVm frameVm, bool bringIntoView)
        {
            this.TrackedFrameVm = frameVm;
            if(bringIntoView)
            {
                this.NavigateToSelectedFrame();
            }
        }

        private void NavigateToSelectedFrame()
        {
            this.CDoubleClickedFrame.Execute(this.TrackedFrameVm);
        }

        private void PacketChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var closestDataPoint = e.Context.ClosestDataPoint.DataPoint as CategoricalDataPoint;
            if(closestDataPoint == null) { return; }
            
            this.TrackedFrameVm = closestDataPoint.DataItem as FrameVm;
        }

        private void RadPacketChart_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.NavigateToSelectedFrame();
        }

        private void RadPacketChart_OnZoomChanged(object sender, ChartZoomChangedEventArgs e)
        {
            if(this.radPacketChartTimeAxis == null) { return; }

            var axis = this.radPacketChartTimeAxis;
            var range = axis.ActualVisibleRange.Maximum - axis.ActualVisibleRange.Minimum;

            var t = (int) (range.TotalSeconds/axis.MajorStep);

            axis.LabelInterval = Math.Max(t/100, 1);

            axis.LabelFormat = "HH:mm:ss.ff" + ((range.TotalMilliseconds <= 100*100)? "ff" : string.Empty);


            if(this._gotoFrameFlagZoom)
            {
                // radPacketChart.Zoom = e.PreviousZoom;
                this._gotoFrameFlagZoom = false;
            }
        }
        private void UpFlowFramesQualityListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.NavigateToSelectedFrame();
        }
    }
}