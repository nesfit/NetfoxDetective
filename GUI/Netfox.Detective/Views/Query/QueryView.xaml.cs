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

using System.Windows;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Views.Windows;

namespace Netfox.Detective.Views.Query
{
    /// <summary>
    ///     Interaction logic for QueryView.xaml
    /// </summary>
    public partial class QueryView : PageViewBase
    {
        private const string DefaultName = "Query";

        public QueryView()
        {
            this.InitializeComponent();

            // MainTabControl.IsEnabled = false;


            this.ViewName = DefaultName;
            //base.ControlVisible = Visibility.Visible;
            // base.ControlVisible = Visibility.Collapsed;

            //DataContext = _conversationsQueryVm;
        }

        public static bool IsFocusable
        {
            get { return true; }
        }

        public static DetectivePaneDockingPosition DetectivePaneDockingPosition
        {
            get { return DetectivePaneDockingPosition.Main; }
        }

        private void ConvHelperButton_OnClick(object sender, RoutedEventArgs e)
        {
            var helperWindow = new QueryableHelper(typeof(Conversation), "Conversation");
            helperWindow.Show();
        }

        private void ExpDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            var helperWindow = new QueryableHelper(typeof(ExportedData), "Exported data");
            helperWindow.Show();
        }

        private void ExpDataSpecificButton_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;

            var queryRecord = frameworkElement.DataContext as ExportedDataQueryVm.RawQueryRecord;

            if(queryRecord != null)
            {
                var helperWindow = new QueryableHelper(queryRecord.TargetRecordType, queryRecord.TargetRecordType.Name);
                helperWindow.Show();
            }
        }

        private void ExpResultButton_OnClick(object sender, RoutedEventArgs e)
        {
            var helperWindow = new QueryableHelper(typeof(ExportResult), "Export result");
            helperWindow.Show();
        }
    }
}