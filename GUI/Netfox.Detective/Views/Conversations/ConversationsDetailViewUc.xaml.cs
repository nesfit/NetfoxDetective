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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Netfox.Core.Helpers;
using Netfox.Detective.ViewModelsDataEntity.Conversations;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;

namespace Netfox.Detective.Views.Conversations
{
    /// <summary>
    ///     Interaction logic for ConversationsDetailViewUc.xaml
    /// </summary>
    public partial class ConversationsDetailViewUc : UserControl, INotifyPropertyChanged
    {
        private IConversationsVm _conversationsVm;
        private bool _conversationsOrderByDuration;
        private bool _conversationsOrderByTraffic;
        private double _horizontalRangeEnd;
        private double _horizontalRangeStart;
        private double _panOffset;
        private bool _showConversationsDuration;
        private bool _showConversationsTraffic;

        public ConversationsDetailViewUc()
        {
            this.ShowConversationsTraffic = true;
            this.ShowConversationsDuration = true;
            this.ConversationsOrderByTraffic = true;

            this._horizontalRangeStart = 0.0;
            this._horizontalRangeEnd = 1.0;

            this.InitializeComponent();

            this.DataContextChanged += (sender, args) =>
            {
                var conversationsVm = this.DataContext as IConversationsVm;
                if(conversationsVm != null) { this.ConversationsVm = conversationsVm; }
            };

            //this.xamPeerMapView.NodeControlAttachedEvent += (sender, e) =>
            //{
            //    e.NodeControl.MouseLeftButtonUp += this.Element_MouseLeftButtonUp;
            //    e.NodeControl.MouseLeftButtonDown += this.Element_MouseLeftButtonDown;
            //    e.NodeControl.MouseMove += this.Element_MouseMove;
            //};

            //this.xamPeerMapView.NodeControlDetachedEvent += (sender, e) =>
            //{
            //    e.NodeControl.MouseLeftButtonUp -= this.Element_MouseLeftButtonUp;
            //    e.NodeControl.MouseLeftButtonDown -= this.Element_MouseLeftButtonDown;
            //    e.NodeControl.MouseMove -= this.Element_MouseMove;
            //};
        }

        public IConversationsVm ConversationsVm
        {
            get { return this._conversationsVm; }
            set
            {
                //if(this._conversationsVm != null) { this._conversationsVm.PropertyChanged -= this.ConversationsVmOnPropertyChanged; }

                this._conversationsVm = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ConversationsByTrafficOrDuration));

                //if(this._conversationsVm != null) { this._conversationsVm.PropertyChanged += this.ConversationsVmOnPropertyChanged; }
            }
        }

        public NotifyTaskCompletion<ConversationVm[]> ConversationsByTrafficOrDuration
        {
            get
            {
                if(this._conversationsVm == null) { return null; }
                var conversationsOrderByTrafficLocal = this._conversationsOrderByTraffic
                    ? this._conversationsVm.ConversationsByTraffic
                    : this._conversationsVm.ConversationsByDuration;

                return conversationsOrderByTrafficLocal;
            }
        }

        public bool ShowConversationsTraffic
        {
            get { return this._showConversationsTraffic; }
            set
            {
                this._showConversationsTraffic = value;

                if(this.CGrid != null && this.CGrid.RowDefinitions.Count == 3)
                {
                    if(!this._showConversationsTraffic) {
                        this.CGrid.RowDefinitions[1].Height = GridLength.Auto;
                    }
                    else
                    {
                        this.CGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                    }
                }
                this.OnPropertyChanged();
            }
        }

        public bool ShowConversationsDuration
        {
            get { return this._showConversationsDuration; }
            set
            {
                this._showConversationsDuration = value;

                if(this.CGrid != null && this.CGrid.RowDefinitions.Count == 3)
                {
                    if(!this._showConversationsDuration) {
                        this.CGrid.RowDefinitions[2].Height = GridLength.Auto;
                    }
                    else
                    {
                        this.CGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                    }
                }
                this.OnPropertyChanged();
            }
        }

        public bool ConversationsOrderByDuration
        {
            get { return this._conversationsOrderByDuration; }
            set
            {
                this._conversationsOrderByDuration = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ConversationsByTrafficOrDuration));
            }
        }

        public bool ConversationsOrderByTraffic
        {
            get { return this._conversationsOrderByTraffic; }
            set
            {
                this._conversationsOrderByTraffic = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ConversationsByTrafficOrDuration));
            }
        }

        public double HorizontalRangeStart
        {
            get { return this._horizontalRangeStart; }
            set
            {
                if(this._horizontalRangeStart != value)
                {
                    this._horizontalRangeStart = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public double HorizontalRangeEnd
        {
            get { return this._horizontalRangeEnd; }
            set
            {
                if(this._horizontalRangeEnd != value)
                {
                    this._horizontalRangeEnd = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public double PanOffset
        {
            get { return this._panOffset; }
            set
            {
                if(this._panOffset != value)
                {
                    this._panOffset = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ConversationVm SelectedConversation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //{
        //    if(e.AddedPoints.Any())
        //    {
        //        var cvm = e.AddedPoints[0].DataItem as ConversationVm;
        //        if(cvm != null)
        //        {
        //            this.SelectedConversation = cvm;
        //            /*
        //            if (sender == Conversationschart)
        //                Conversationschart2.SelectedPoints = new ReadOnlyDataPointCollection(e.AddedPoints);*/
        //        }
        //    }
        //}

        //private void ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        //{
        //    var closestDataPoint = e.Context.ClosestDataPoint;
        //    if(closestDataPoint != null)
        //    {
        //        var data = closestDataPoint.DataPoint.DataItem as KeyValue<DateTime, long>;
        //        this._trackedFrameVm = data.Ref as FrameVm;
        //        this.radTimelineTimeInfo.Text = data.Key.ToString();
        //        this.radTimelineBytesInfo.Text = data.Value.ToString();
        //    }
        //}

        //private void Conversationchart_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)

        //private void ChartSelectionBehavior_OnSelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        //{
        //    if(this._trackedFrameVm != null)
        //    {
        //        this.ConversationsVm.SetCurrentConversationByFrame(this._trackedFrameVm);
        //        BringToFrontMessage.SendBringToFrontMessage("ConversationDetailView");
        //    }
        //}

        //private void Conversationchart_SelectionChanged(object sender, ChartSelectionChangedEventArgs e) { }

        //private void Conversations_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if(this.SelectedConversation != null)
        //    {
        //        ConversationMessage.SendConversationMessage(this.SelectedConversation, ConversationMessage.MessageType.CurrentConversationChanged, false);

        //        BringToFrontMessage.SendBringToFrontMessage("ConversationDetailView");
        //    }
        //}

        //private void Conversations_OnTrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        //{
        //    if(e.Context.DataPointInfos.Any())
        //    {
        //        var cvm = e.Context.DataPointInfos[0].DataPoint.DataItem as ConversationVm;
        //        if(cvm != null) { this.SelectedConversation = cvm; }
        //    }
        //}

        //private void Conversationschart_OnZoomChanged(object sender, ChartZoomChangedEventArgs e) { }
        //private void Conversationschart_SelectionChanged(object sender, ChartSelectionChangedEventArgs e) { }
        //private void Conversationschart2_OnZoomChanged(object sender, ChartZoomChangedEventArgs e) { }

        //private void ConversationsVmOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        //{
        //    if(propertyChangedEventArgs.PropertyName == "ConversationsByTraffic" || propertyChangedEventArgs.PropertyName == "ConversationsByDuration") {
        //        this.OnPropertyChanged("ConversationsByTrafficOrDuration");
        //    }
        //}

        //private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var element = (NetworkNodeNodeControl) sender;
        //    this._currentElement = element; // keep track of which node this is
        //    element.CaptureMouse();
        //    this._isMoveInEffect = true; // initiate the movement effect
        //    this._currentPosition = e.GetPosition(element.Parent as UIElement); // keep track of position
        //}

        //private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    var element = (NetworkNodeNodeControl) sender;
        //    element.ReleaseMouseCapture();
        //    this._isMoveInEffect = false; // terminate the movement effect
        //}

        //private void Element_MouseMove(object sender, MouseEventArgs e)
        //{
        //    var element = (NetworkNodeNodeControl) sender;
        //    if(this._currentElement == null || element != this._currentElement)
        //    {
        //        // this might happen if a node is released outside of the view area.
        //        // terminate the movement effect.
        //        this._isMoveInEffect = false;
        //    }
        //    else if(this._isMoveInEffect) // is the movement effect active?
        //    {
        //        if(e.GetPosition(this.xamPeerMapView).X > this.xamPeerMapView.ActualWidth || e.GetPosition(this.xamPeerMapView).Y > this.xamPeerMapView.ActualHeight
        //           || e.GetPosition(this.xamPeerMapView).Y < 0.0)
        //        {
        //            // drag is outside of the allowable area, so release the element
        //            element.ReleaseMouseCapture();
        //            this._isMoveInEffect = false;
        //        }
        //        else
        //        {
        //            // drag is within the allowable area, so update the element's position
        //            var currentPosition = e.GetPosition(element.Parent as UIElement);

        //            element.Node.Location = new Point(element.Node.Location.X + (currentPosition.X - this._currentPosition.X)/this.xamPeerMapView.ZoomLevel,
        //                element.Node.Location.Y + (currentPosition.Y - this._currentPosition.Y)/this.xamPeerMapView.ZoomLevel);

        //            this._currentPosition = currentPosition;
        //        }
        //    }
        //}

        //private void Redraw_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    // re-draw nodes
        //    this.xamPeerMapView.UpdateNodeArrangement();
        //}
    }
}