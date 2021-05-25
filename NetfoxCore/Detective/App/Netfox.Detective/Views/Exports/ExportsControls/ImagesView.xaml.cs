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
using System.Windows.Input;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.Views.Exports.ExportsControls
{
    /// <summary>
    ///     Interaction logic for ImagesView.xaml
    /// </summary>
    public partial class ImagesView : UserControl, INotifyPropertyChanged
    {
        private int _boxHeight;
        private int _boxWidth;
        //  public ExportVm context;
        public ImagesView() { this.InitializeComponent(); }

        public int BoxHeight
        {
            get { return this._boxHeight; }
            set
            {
                this._boxHeight = value;
                this.OnPropertyChanged();
            }
        }

        public int BoxWidth
        {
            get { return this._boxWidth; }
            set
            {
                this._boxWidth = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        private void ImagesListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var image = this.ImagesListBox.SelectedItem as ExportResultHelper.ResultImage;

            if(image != null && image.ResultVm != null)
            {
                image.ResultVm.SelectDataByDataObject(image.DataVm, true);
              
                new DetectiveMvvmLightMessenger().AsyncSend(new SelectedExportResultMessage
                {
                    ExportVm = image.ResultVm
                });
            }
        }

        private void SldZoom_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BoxWidth = (int) this.sldZoom.Value;
            this.BoxHeight = (int) this.sldZoom.Value + 50;
        }
    }
}