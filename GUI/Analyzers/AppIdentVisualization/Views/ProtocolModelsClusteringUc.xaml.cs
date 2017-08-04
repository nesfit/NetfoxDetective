// Copyright (c) 2017 Jan Pluskal
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Controls.Maps;

namespace Netfox.AnalyzerAppIdent.Views
{
    /// <summary>
    /// Interaction logic for ProtocolModelsClusteringUc.xaml
    /// </summary>
    public partial class ProtocolModelsClusteringUc : UserControl
    {

        public ProtocolModelsClusteringUc()
        {
            this.InitializeComponent();

            #region xamNetworkNode manual realocation
            this.xnn.NodeControlAttachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonDown += this.Element_MouseLeftButtonDown;
                e.NodeControl.MouseMove += this.Element_MouseMove;
                e.NodeControl.MouseLeftButtonUp += this.Element_MouseLeftButtonUp;
            };

            this.xnn.NodeControlDetachedEvent += (sender, e) =>
            {
                e.NodeControl.MouseLeftButtonDown -= this.Element_MouseLeftButtonDown;
                e.NodeControl.MouseMove -= this.Element_MouseMove;
                e.NodeControl.MouseLeftButtonUp -= this.Element_MouseLeftButtonUp;
            };
            #endregion

        }

        #region xamNetworkNode manual realocation
        private bool _isMoveInEffect; // is the movement in effect?
        private NetworkNodeNodeControl _currentElement; // the element that we are moving
        private Point _currentPosition; // the current position of that element

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            this._currentElement = element; // keep track of which node this is
            element.CaptureMouse();
            this._isMoveInEffect = true; // initiate the movement effect
            this._currentPosition = e.GetPosition(element.Parent as UIElement); // keep track of position
        }
        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            if (this._currentElement == null || element != this._currentElement)
            {
                // this might happen if a node is released outside of the view area.
                // terminate the movement effect.
                this._isMoveInEffect = false;
            }
            else if (this._isMoveInEffect) // is the movement effect active?
            {
                if (e.GetPosition(this.xnn).X > this.xnn.ActualWidth || e.GetPosition(this.xnn).Y > this.xnn.ActualHeight || e.GetPosition(this.xnn).Y < 0.0)
                {
                    // drag is outside of the allowable area, so release the element
                    element.ReleaseMouseCapture();
                    this._isMoveInEffect = false;
                }
                else
                {
                    // drag is within the allowable area, so update the element's position
                    var currentPosition = e.GetPosition(element.Parent as UIElement);

                    element.Node.Location = new Point(
                        element.Node.Location.X + (currentPosition.X - this._currentPosition.X) /this.xnn.ZoomLevel,
                        element.Node.Location.Y + (currentPosition.Y - this._currentPosition.Y) /this.xnn.ZoomLevel);

                    this._currentPosition = currentPosition;
                }
            }
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (NetworkNodeNodeControl)sender;
            element.ReleaseMouseCapture();
            this._isMoveInEffect = false; // terminate the movement effect
        }
        #endregion
    }
}
