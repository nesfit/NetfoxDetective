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

using System.Windows.Controls;

namespace Netfox.Detective.Views.Exports.ExportsControls
{
    /// <summary>
    ///     Interaction logic for CallsView.xaml
    /// </summary>
    public partial class CallsView : UserControl
    {
        public CallsView() { this.InitializeComponent(); }

        //private void CallsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var call = this.MessagesDataGrid.SelectedItem as ExportResultHelper.Call;

        //    if(call != null && call.ResultVm != null)
        //    {
        //        call.ResultVm.SelectDataByDataObject(call.DataVm, true);
        //        ExportResultMessage.SendExportResultMessage(call.ResultVm, ExportResultMessage.MessageType.ExportResultSelected);
        //        BringToFrontMessage.SendBringToFrontMessage("ExportContentView");
        //    }
        //}
    }
}