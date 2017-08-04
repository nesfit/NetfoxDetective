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
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Core.Messages.Views;

namespace Netfox.Detective.Views.FullTextSearch
{
    /// <summary>
    ///     Interaction logic for FullTextSearchResultsView.xaml
    /// </summary>
    public partial class FullTextSearchResultsView : PageViewBase
    {
        private string baseName = "Search result";
        private SearchResultVm resultVm;

        public FullTextSearchResultsView(SearchResultVm resultVm)
        {
            this.InitializeComponent();

            this.resultVm = resultVm;
            this.DataContext = resultVm;

            this.UpdateName();
            resultVm.FulltextSearchResult.PropertyChanged += this.resultVm_PropertyChanged;
            resultVm.PropertyChanged += this.resultVm_PropertyChanged;

            if(!resultVm.HasExportedDataResults) { this.ResultsControl.SelectedIndex = 1; }
        }

        public static bool IsFocusable
        {
            get { return true; }
        }

        public static DetectivePaneDockingPosition DetectivePaneDockingPosition
        {
            get { return DetectivePaneDockingPosition.None; }
        }

        private void resultVm_PropertyChanged(object sender, PropertyChangedEventArgs e) { if(e.PropertyName == "Name") { this.UpdateName(); } }
        private void SearchResultCapture_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) { BringToFrontMessage.SendBringToFrontMessage("FrameContentView"); }
        private void SearchResultExportedData_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) { BringToFrontMessage.SendBringToFrontMessage("ExportResultView"); }

        private void SearchResultFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if(frameworkElement == null) { return; }

            var searchresult = frameworkElement.DataContext as ExportedDataSearchResult;

            if(searchresult != null && !string.IsNullOrEmpty(searchresult.Path)) { if(File.Exists(searchresult.Path)) { Process.Start(searchresult.Path); } }
        }

        private void UpdateName() { this.ViewName = string.Format("{0} - {1}", this.baseName, this.resultVm.FulltextSearchResult.Name); }
    }
}