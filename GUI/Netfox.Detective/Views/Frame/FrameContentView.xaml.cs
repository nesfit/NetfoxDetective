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
using System.Windows;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Core.Messages.Base;
using Netfox.Detective.ViewModels.Frame;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;

namespace Netfox.Detective.Views.Frame
{
    /// <summary>
    ///     Interaction logic for FrameContentView.xaml
    /// </summary>
    public partial class FrameContentView : DetectiveDataEntityPaneViewBase, IFrameContentView
    {
        private const string DefaultName = "Frame - content";
        private CaptureVm _captureVm;
        private FrameVm _context;

        public FrameContentView()
        {
            this.InitializeComponent();

            //Task.Factory.StartNew(() =>
            //{
            //    Messenger.Default.Register<FrameMessage>(this, message => this.FrameActionHandler(message));
            //    Messenger.Default.Register<CaptureMessage>(this, message => this.CaptureActionHandler(message));
            //});
        }

        private void CaptureActionHandler(CaptureMessage message)
        {
            if(message.Type == CaptureMessage.MessageType.CurrentCaptureChanged) { this._captureVm = message.CaptureVm as CaptureVm; }
        }

        private void context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectedOffset" || e.PropertyName == "SelectedLength") { this.UpdateSelection(); }
        }

        private void FrameActionHandler(FrameMessage message)
        {
            if(message.Type == FrameMessage.MessageType.CurrentFrameChanged)
            {
                if(this._context != null) { this._context.PropertyChanged -= this.context_PropertyChanged; }

                this._context = message.Frame as FrameVm;

                if(this._context != null) { this._context.PropertyChanged += this.context_PropertyChanged; }

                this.DataContext = this._context;

                this.Visibility = (this._context != null? Visibility.Visible : Visibility.Hidden);
                //this.ControlVisible = (this._context != null? Visibility.Visible : Visibility.Collapsed);
                this.GenericFiledsView.UpdateExpandState();
                this.GenericFiledsView.FrameVm = (FrameVm) message.Frame;
                this.radHexDetailsDataGrid.DataContext = this._context;
                this.UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            /*  radHexDetailsDataGrid.DataContext = context;

            if (context != null)
            {
                var collection = radHexDetailsDataGrid.Items;

                int offset = 0;
                foreach (var dataItem in collection)
                {
                    int colIndex = 0;
                    foreach (var col in radHexDetailsDataGrid.Columns)
                    {
                        var content1 = col.GetCellContent(dataItem);

                        if (content1 != null)
                        { 
                            DataGridCell cell = content1.Parent as DataGridCell;
                            if (colIndex > 0 && colIndex < 9)
                            {
                                if (offset >= context.SelectedOffset &&
                                    offset <= context.SelectedOffset + context.SelectedLength)
                                {
                                    cell.Background = new SolidColorBrush(Colors.DodgerBlue);
                                    cell.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else
                                {
                                    cell.Background = new SolidColorBrush(Colors.Transparent);
                                    cell.Foreground = new SolidColorBrush(Colors.Black);
                                }


                                offset++;
                            }


                            colIndex++;
                        }

 

                    }
                }
            } */
        }
    }
}