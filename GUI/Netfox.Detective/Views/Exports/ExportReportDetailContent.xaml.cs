// The MIT License (MIT)
//  
// Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
// Author(s):
// Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Netfox.Detective.Core.Messages.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.Views.Exports.Notifications;

namespace Netfox.Detective.Views.Exports
{
    /// <summary>
    ///     Interaction logic for ExportReportDetailConent.xaml
    /// </summary>
    public partial class ExportReportDetailContent : UserControl, INotifyPropertyChanged
    {
        public ExportReportDetailContent()
        {
            this.InitializeComponent();

            Messenger.Default.Register<ExportReportViewMessage>(this, message => this.ExportReportViewMessageHandler(message));

            this.DataContext = this;
        }

        public bool IsHidden { get; set; }
        private ExportReportVm CurrentExportReportVm { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if(handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        private void ExportReportViewMessageHandler(ExportReportViewMessage message)
        {
            if(message.ExportReport != null)
            {
                this.DataContext = message.ExportReport;

                if(message.MessageType == ExportReportViewMessage.Type.Show || message.MessageType == ExportReportViewMessage.Type.SelectAndShow) { this.IsHidden = false; }

                this.CurrentExportReportVm = message.ExportReport;
            }
            else
            { this.IsHidden = true; }
            this.OnPropertyChanged("IsHidden");
        }

        private void FramesListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(this.CurrentExportReportVm != null)
            {
                this.CurrentExportReportVm.CurrentFrameId = ((uint) this.FramesListBox.SelectedItem);
                this.CurrentExportReportVm.SelectFrame(true);
            }
        }

        private void FramesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.CurrentExportReportVm != null && this.FramesListBox.SelectedItem != null)
            {
                this.CurrentExportReportVm.CurrentFrameId = ((uint) this.FramesListBox.SelectedItem);
                this.CurrentExportReportVm.SelectFrame(false);
            }
        }

        private void ShowExportedDataBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if(this.CurrentExportReportVm != null && this.CurrentExportReportVm.IsExportedDataReport)
            {
                var edataVm = this.CurrentExportReportVm.OrgObject as ExportDataVm;
                if(edataVm != null) { ExportDataMessage.SendExportDataMessage(edataVm, ExportDataMessage.MessageType.DataSelected); }
            }
        }

        private void ShowExportResultBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if(this.CurrentExportReportVm != null && this.CurrentExportReportVm.IsExportResultReport)
            {
                var eresVm = this.CurrentExportReportVm.OrgObject as ExportResultVm;
                if(eresVm != null) { ExportResultMessage.SendExportResultMessage(eresVm, ExportResultMessage.MessageType.ExportResultSelected); }
            }
        }
    }
}